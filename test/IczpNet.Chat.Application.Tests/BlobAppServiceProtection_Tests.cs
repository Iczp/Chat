using IczpNet.Chat.Blobs;
using IczpNet.Chat.Blobs.Dtos;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;
using Xunit;

namespace IczpNet.Chat;

public class BlobAppServiceProtection_Tests
{
    private sealed class ExposedBlobAppService(IRepository<Blob, Guid> repository) : BlobAppService(repository)
    {
        public Task CallCheckCreateAsync(BlobCreateInput input) => CheckCreateAsync(input);
        public Task CallCheckUpdateAsync(Guid id, Blob entity, BlobUpdateInput input) => CheckUpdateAsync(id, entity, input);
        protected override Task CheckPolicyAsync(string policyName) => Task.CompletedTask;
    }

    private sealed class TestBlobCreateInput(string container) : BlobCreateInput
    {
        public override string Container => container;
    }

    private static (ExposedBlobAppService Service, Blob ChatBlob, Blob AvatarBlob) CreateService()
    {
        var chatBlobId = Guid.NewGuid();
        var chatBlob = new Blob(chatBlobId, "chat-files")
        {
            FileName = "secret.pdf",
            IsPublic = false,
            Status = BlobStatus.Bound,
            RefCount = 1,
            UploaderChatObjectId = 101L,
        };

        var avatarBlobId = Guid.NewGuid();
        var avatarBlob = new Blob(avatarBlobId, "user-portraits")
        {
            FileName = "avatar.png",
            IsPublic = true,
            IsStatic = false,
            Status = BlobStatus.Temp,
            RefCount = 0,
        };

        var store = new List<Blob> { chatBlob, avatarBlob };

        var blobRepo = Substitute.For<IRepository<Blob, Guid>>();
        blobRepo.FindAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => store.FirstOrDefault(x => x.Id == call.Arg<Guid>()));
        blobRepo.GetAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => store.First(x => x.Id == call.Arg<Guid>()));
        blobRepo.GetQueryableAsync().Returns(_ => Task.FromResult(store.AsQueryable()));
        blobRepo.DeleteAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        blobRepo.DeleteAsync(Arg.Any<Blob>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        blobRepo.DeleteManyAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var refRepo = Substitute.For<IRepository<BlobMessageRef, Guid>>();
        refRepo.AnyAsync(Arg.Any<Expression<Func<BlobMessageRef, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var permChecker = Substitute.For<IPermissionChecker>();
        permChecker.IsGrantedAsync(Arg.Any<string>()).Returns(true);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.Id.Returns(Guid.NewGuid());

        var objectMapper = Substitute.For<IObjectMapper>();

        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<IRepository<BlobMessageRef, Guid>>().Returns(refRepo);
        lazy.LazyGetRequiredService<IPermissionChecker>().Returns(permChecker);
        lazy.LazyGetRequiredService<ICurrentUser>().Returns(currentUser);
        lazy.LazyGetService<ICurrentUser>().Returns(currentUser);
        lazy.LazyGetRequiredService<IObjectMapper>().Returns(objectMapper);

        var service = new ExposedBlobAppService(blobRepo)
        {
            LazyServiceProvider = lazy,
        };

        return (service, chatBlob, avatarBlob);
    }

    [Fact]
    public async Task Generic_BlobAppService_blocks_privatized_chat_blob_public_flip_and_direct_delete()
    {
        var (service, chatBlob, avatarBlob) = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CallCheckCreateAsync(new TestBlobCreateInput("chat-files")
            {
                FileName = "leak.pdf",
                IsPublic = true,
            }));

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CallCheckUpdateAsync(chatBlob.Id, chatBlob, new BlobUpdateInput
            {
                FileName = "secret.pdf",
                IsPublic = true,
                IsStatic = false,
            }));

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.DeleteAsync(chatBlob.Id));

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.DeleteManyAsync([avatarBlob.Id, chatBlob.Id]));

        // Non-chat public avatar remains editable/deletable
        await service.CallCheckUpdateAsync(avatarBlob.Id, avatarBlob, new BlobUpdateInput
        {
            FileName = "avatar.png",
            IsPublic = true,
            IsStatic = false,
        });
        await service.DeleteAsync(avatarBlob.Id);
    }
}
