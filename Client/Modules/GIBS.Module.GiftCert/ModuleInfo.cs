using Oqtane.Models;
using Oqtane.Modules;

namespace GIBS.Module.GiftCert
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "GiftCert",
            Description = "Gift Certificate",
            Version = "1.0.2",
            ServerManagerType = "GIBS.Module.GiftCert.Manager.GiftCertManager, GIBS.Module.GiftCert.Server.Oqtane",
            ReleaseVersions = "1.0.0,1.0.1,1.0.2",
            Dependencies = "GIBS.Module.GiftCert.Shared.Oqtane,PayPalServerSDK,PdfSharp,MimeKit,MailKit",
            PackageName = "GIBS.Module.GiftCert"
        };
    }
}
