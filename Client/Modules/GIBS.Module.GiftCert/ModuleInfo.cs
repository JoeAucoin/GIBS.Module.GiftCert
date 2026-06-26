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
            Version = "10.1.0",
            ServerManagerType = "GIBS.Module.GiftCert.Manager.GiftCertManager, GIBS.Module.GiftCert.Server.Oqtane",
            ReleaseVersions = "1.0.0,1.0.1,1.0.2,10.1.0",
            Dependencies = "GIBS.Module.GiftCert.Shared.Oqtane,PayPalServerSDK,PdfSharp,MimeKit,MailKit,Oqtane.Licensing.Client.Oqtane,Oqtane.Licensing.Shared.Oqtane",
            PackageName = "GIBS.Module.GiftCert"
        };
    }
}
