using IczpNet.Chat.Contacts.Dtos;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Contacts;

public interface IContactsAppService
{
    Task<PagedResultDto<ContactsDto>> GetListAsync(ContactsGetListInput input);

    Task<PagedResultDto<ContactsIndexDto>> GetIndexsAsync(ContactsGetListInput input);
}
