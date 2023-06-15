using IczpNet.Chat.Cantacts.Dtos;
using IczpNet.Chat.Contacts.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Contacts
{
    public interface IContactsAppService
    {
        Task<PagedResultDto<ContactsDto>> GetListAsync(ContactsGetListInput input);
    }
}
