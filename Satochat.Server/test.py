from requests import Request, Session
from requests.auth import AuthBase
import json
import base64
import hashlib
import os
from Crypto.PublicKey import RSA
from Crypto.Random import get_random_bytes
from Crypto.Cipher import AES, PKCS1_OAEP
from Crypto.Util import Padding
import urllib.parse
from sseclient import SSEClient
import threading

proxies = {
    #'http': 'http://localhost:8080',
    #'https': 'http://localhost:8080',
}

class SatoAuth(AuthBase):
    def __init__(self, token):
        self.token = token

    def __call__(self, req):
        req.headers["Authorization"] = "Bearer %s" % self.token
        return req

class SatoApiResult(object):
    def __init__(self, response):
        self.response = response

    def get_status(self):
        return self.response.status_code

    def get_text(self):
        return self.response.text

    def get_response(self):
        return self.response

    def get_object(self):
        text = self.get_text()
        return json.loads(text) if text else None

class CredentialCallbackResult(object):
    def __init__(self, username, password):
        self.username = username
        self.password = password

class SatoApi(object):
    def __init__(self, endpoint, credential_callback):
        self.endpoint = endpoint
        self.token = None
        self.credential_callback = credential_callback

    def set_token(self, token):
        self.token = token

    def make_url(self, path):
        return self.endpoint + "/" + path

    def make_auth(self):
        return SatoAuth(self.token) if self.token else None

    def request(self, method, path, content=None, params=None, stream=False, recursing=False):
        headers = {"Content-Type": "application/json"}
        content = json.dumps(content) if content else None
        session = Session()
        req = Request(method, self.make_url(path), data=content, auth=self.make_auth(), headers=headers, params=params)
        prepared_req = req.prepare()
        response = session.send(prepared_req, stream=stream, proxies=proxies)

        if response.status_code != 200:
            if response.status_code == 401 and not recursing:
                cred = self.credential_callback()
                token_response = self.get_token(cred.username, cred.password)
                if token_response.get_status() != 200:
                    raise Exception("Authentication failed")
                self.set_token(token_response.get_object()["token"])
                # Retry original request
                return self.request(method, path, content=content, params=params, stream=stream, recursing=True)
            elif response.status_code >= 400 and response.status_code <= 499:
                raise Exception("Client error ({}): {}".format(response.status_code, response.text))
            elif response.status_code >= 500 and response.status_code <= 599:
                raise Exception("Server error ({}): {}".format(response.status_code, response.text))
            else:
                raise Exception("Unexpected status code ({}): {}".format(response.status_code, response.text))

        result = SatoApiResult(response)
        return result

    def get(self, path, params=None, stream=False):
        return self.request("get", path, params=params, stream=stream)

    def post(self, path, content, params=None, stream=False):
        return self.request("post", path, content=content, params=params, stream=stream)

    def create_conversation(self, recipients):
        recipients = [recipient.get_uuid() for recipient in recipients]
        return self.post("conversation", {"recipients": recipients}).get_object()["uuid"]

    def get_token(self, username, password):
        return self.post("token", {"username": username, "password": password})

    def whoami(self):
        return self.get("whoami")

    def send_message(self, conversation, recipients, content):
        encoded_messages = []
        for recipient in recipients:
            encoded_message = self._encode_message(recipient, content)
            encoded_messages += [encoded_message]
        return self.post("message", {"conversation": conversation.get_uuid(), "variants": encoded_messages})

    def get_messages(self, conversation, user):
        params = {"conversation": conversation.get_uuid()}
        response = self.get("message", params=params).get_object()
        decoded_messages = [self._decode_message(encoded_message, user.get_private_key()) for encoded_message in response["messages"]]
        return decoded_messages

    def notify_message_received(self, message_uuid):
        return self.post("message/received/{}".format(message_uuid), None)

    def listen_events(self, on_event):
        result = self.get("event", stream=True)
        with result.get_response() as response:
            lines = []
            for line in response.iter_lines():
                if line and len(line) > 0:
                    lines += [line.decode("utf8")]
                else:
                    data = "\n".join(lines)
                    on_event(data)
                    lines = []

    def _encode_message(self, recipient, content):
        nonce = base64.b64encode(get_random_bytes(32)).decode("ascii")
        payload = json.dumps({"content": content, "nonce": nonce})
        payload_bytes = payload.encode("utf8")
        payload_digest = base64.b64encode(hashlib.sha256(payload_bytes).digest()).decode("ascii")

        secret = get_random_bytes(32)
        iv = get_random_bytes(16)

        aes_cipher = AES.new(secret, AES.MODE_CBC, iv=iv)
        rsa_cipher = PKCS1_OAEP.new(recipient.get_public_key())

        encrypted_payload_bytes = aes_cipher.encrypt(Padding.pad(payload_bytes, 16, "pkcs7"))
        encrypted_secret = rsa_cipher.encrypt(secret)
        encoded_secret = base64.b64encode(encrypted_secret).decode("ascii")
        encoded_payload = base64.b64encode(encrypted_payload_bytes).decode("ascii")
        encoded_iv = base64.b64encode(iv).decode("ascii")

        return {"recipient": recipient.get_uuid(), "digest": payload_digest, "payload": encoded_payload, "iv": encoded_iv, "key": encoded_secret}

    def _decode_message(self, message, private_key):
        expected_digest = base64.b64decode(message["digest"])
        iv = base64.b64decode(message["iv"])
        encrypted_secret = base64.b64decode(message["key"])
        encrypted_payload_bytes = base64.b64decode(message["payload"])

        rsa_cipher = PKCS1_OAEP.new(private_key)
        secret = rsa_cipher.decrypt(encrypted_secret)

        aes_cipher = AES.new(secret, AES.MODE_CBC, iv=iv)
        payload_bytes = aes_cipher.decrypt(encrypted_payload_bytes)
        payload_bytes = Padding.unpad(payload_bytes, 16, "pkcs7")

        actual_digest = hashlib.sha256(payload_bytes).digest()
        if actual_digest != expected_digest:
            raise Exception("Digest mismatch")
            
        payload = json.loads(payload_bytes.decode("utf8"))
        return {"author": message["author"], "timetamp": message["timestamp"], "content": payload["content"], "uuid": message["uuid"]}

class EventListener(threading.Thread):
    def __init__(self, api, on_event):
        super().__init__()
        self.api = api
        self.on_event = on_event

    def run(self):
        api.listen_events(alias, self.on_event)

class Credential(object):
    def __init__(self, username, password):
        self.username = username
        self.password = password

    def get_username(self):
        return self.username

    def get_password(self):
        return self.password

class User(object):
    def __init__(self, credential, private_key_passphrase):
        self.credential = credential
        self.uuid = None
        self.private_key_passphrase = private_key_passphrase

        key_pair = RSA.generate(2048)
        self.private_key = key_pair.exportKey(format='PEM', passphrase=private_key_passphrase, pkcs=8, protection="scryptAndAES256-CBC")
        self.public_key = key_pair.publickey().exportKey(format='PEM')

    def get_uuid(self):
        return self.uuid

    def get_private_key(self):
        return RSA.import_key(self.private_key, passphrase=self.private_key_passphrase)

    def get_public_key(self):
        return RSA.import_key(self.public_key)

    def credential_callback(self):
        return CredentialCallbackResult(self.credential.get_username(), self.credential.get_password())

class Participant(object):
    def __init__(self, uuid, public_key):
        self.uuid = uuid
        self.public_key = public_key

    def get_uuid(self):
        return self.uuid

    def get_public_key(self):
        return self.public_key

class LocalParticipant(Participant):
    def __init__(self, uuid, public_key, private_key):
        super().__init__(uuid, public_key)
        self.private_key = private_key

    def get_private_key(self):
        return self.private_key

class Conversation(object):
    def __init__(self, uuid):
        self.uuid = uuid

    def get_uuid(self):
        return self.uuid

class TestUser(User):
    def __init__(self, credential, private_key_passphrase):
        super().__init__(credential, private_key_passphrase)
        self.api = None
        self.event_listener = None

    def init_api(self, endpoint, on_event):
        api = SatoApi(endpoint, self.credential_callback)
        uuid = api.whoami().get_object()["uuid"]
        # TODO: upload public key
        self.api = api
        self.uuid = uuid
        self.event_listener = EventListener(api, on_event)

    def get_api(self):
        return self.api

    def listen_events(self):
        self.event_listener.start()

class TestSatoApi(object):
    def __init__(self):
        self.endpoint = "http://localhost:63154"

        self.users = {
            "satoshi": TestUser(Credential("satoshi", "12345678"), "passphrase"),
            "sachiko": TestUser(Credential("sachiko", "12345678"), "passphrase"),
            "dummy": TestUser(Credential("dummy", "12345678"), "passphrase")
        }

        for alias in self.users:
            user = self.users[alias]
            user.init_api(self.endpoint, lambda data: self.on_event(alias, data))

    def create_conversation(self, author, recipients):
        recipients = [Participant(self.users[uuid].get_uuid(), self.users[uuid].get_public_key()) for uuid in recipients]
        author = self.users[author]
        api = author.get_api()
        return api.create_conversation(recipients)

    def send_message(self, author, conversation, recipients, content):
        recipients = [Participant(self.users[uuid].get_uuid(), self.users[uuid].get_public_key()) for uuid in recipients]
        author = self.users[author]
        api = author.get_api()
        api.send_message(Conversation(conversation), recipients, content)

    def get_messages(self, user, conversation):
        user = self.users[user]
        api = user.get_api()
        decoded_messages = api.get_messages(Conversation(conversation), LocalParticipant(user.get_uuid(), user.get_public_key(), user.get_private_key()))
        return decoded_messages

    def notify_message_received(self, user, message):
        user = self.users[user]
        api = user.get_api()
        api.notify_message_received(message)

    def listen_events(self, user, listener):
        user = self.users[user]
        api = user.get_api()
        api.listen_events(listener)

    def on_event(self, user, data):
        print("{} received an event: {}".format(user, data))

api = TestSatoApi()

for alias in api.users:
    user = api.users[alias]
    user.listen_events()

for alias in api.users:
    user = api.users[alias]
    print("{}: {}".format(alias, user.get_uuid()))

conversation_uuid = api.create_conversation("satoshi", ["sachiko", "dummy"])
conversations = [conversation_uuid,
                 api.create_conversation("sachiko", ["satoshi", "dummy"]),
                 api.create_conversation("dummy", ["satoshi", "sachiko"])]
if conversations.count(conversation_uuid) != len(conversations):
    raise Exception("Same users resulted in a different conversation UUID")

print("Conversation: {}".format(conversation_uuid))

messages_to_send = [
    {"author": "satoshi", "content": "hello", "conversation": conversation_uuid, "recipients": ["sachiko", "dummy"]},
    {"author": "sachiko", "content": "world", "conversation": conversation_uuid, "recipients": ["satoshi", "dummy"]},
    {"author": "dummy", "content": "baka", "conversation": conversation_uuid, "recipients": ["satoshi", "sachiko"]}
]

for msg in messages_to_send:
    api.send_message(msg["author"], msg["conversation"], msg["recipients"], msg["content"])
    print("{} sent a message: {}".format(msg["author"], msg["content"]))

exit(0)

for alias in api.users:
    messages = api.get_messages(alias, conversation_uuid)
    messages_json = json.dumps(messages, indent=4)
    print("New messages for {}:\n{}".format(alias, messages_json))

for alias in api.users:
    messages = api.get_messages(alias, conversation_uuid)
    if len(messages) == 0:
        raise Exception("Messages should still exist on server")

for alias in api.users:
    messages = api.get_messages(alias, conversation_uuid)
    for msg in messages:
        api.notify_message_received(alias, msg["uuid"])
        print("{} notified that message was received: {}".format(alias, msg["uuid"]))

for alias in api.users:
    messages = api.get_messages(alias, conversation_uuid)
    if len(messages) != 0:
        raise Exception("Messages should no longer exist on server")
    messages_json = json.dumps(messages, indent=4)
    print("New messages for {}:\n{}".format(alias, messages_json))

