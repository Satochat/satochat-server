using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Satochat.Server.Models;
using Satochat.Server.Services;
using Microsoft.EntityFrameworkCore;
using Satochat.Server.ViewModels;
using Satochat.Shared.Crypto;

namespace Satochat.Server.Controllers {
    [Route("key")]
    public class KeyController : BaseController {
        private readonly SatochatContext _dbContext;

        public KeyController(SatochatContext dbContext) {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("public")]
        public async Task<IActionResult> GetPublicKey(KeyViewModelAspnet.GetPublicKey model) {
            var user = await _dbContext.Users.Include(e => e.PublicKeys).SingleOrDefaultAsync(e => e.Uuid == getUserUuid());
            if (user == null) {
                throw new ServiceException(ServiceErrorCode.NotFound);
            }

            var friend = await _dbContext.Users.Include(e => e.PublicKeys).SingleOrDefaultAsync(e => e.Uuid == model.Uuid);
            if (friend == null) {
                throw new ServiceException(ServiceErrorCode.NotFound);
            }

            var publicKeyEntity = friend.PublicKeys.SingleOrDefault();
            if (publicKeyEntity == null) {
                return NotFound();
            }

            return Ok(new KeyViewModelAspnet.GetPublicKeyResult(publicKeyEntity.Key));
        }

        [HttpPost]
        [Route("public")]
        public async Task<IActionResult> PostPublicKey([FromBody]KeyViewModelAspnet.PutPublicKey model) {
            var user = await _dbContext.Users.Include(e => e.PublicKeys).SingleOrDefaultAsync(e => e.Uuid == getUserUuid());
            if (user == null) {
                throw new ServiceException(ServiceErrorCode.NotFound);
            }

            var publicKeyEntity = user.PublicKeys.SingleOrDefault();
            if (publicKeyEntity != null) {
                _dbContext.Remove(publicKeyEntity);
            }

            var publicKey = SatoPublicKey.FromPem(model.Key);

            user.PublicKeys.Add(new UserPublicKey(publicKey.ToPem()));
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}