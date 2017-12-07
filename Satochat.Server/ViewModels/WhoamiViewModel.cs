namespace Satochat.Server.ViewModels {
    public static class WhoamiViewModelAspnet {
        public class ViewResult {
            public string Uuid { get; set; }

            public ViewResult(string uuid) {
                Uuid = uuid;
            }
        }
    }
}
