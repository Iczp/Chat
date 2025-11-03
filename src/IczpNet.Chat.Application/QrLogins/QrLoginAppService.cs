using IczpNet.Chat.BaseAppServices;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace IczpNet.Chat.QrLogins;

public class QrLoginAppService(IQrLoginManager qrLoginManager) : ChatAppService, IQrLoginAppService
{
    public IQrLoginManager QrLoginManager { get; } = qrLoginManager;



}
