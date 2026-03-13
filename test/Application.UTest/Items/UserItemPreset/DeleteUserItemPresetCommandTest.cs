using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class DeleteUserItemPresetCommandTest : TestBase
{
    [Test]
    public async Task Delete()
    {
        var user = ArrangeDb.Users.Add(new User()).Entity;
        var preset = ArrangeDb.UserItemPresets.Add(new UserItemPreset
        {
            UserId = user.Id,
            Name = "to-delete",
        }).Entity;
        await ArrangeDb.SaveChangesAsync();

        var result = await new DeleteUserItemPresetCommand.Handler(ActDb)
            .Handle(new DeleteUserItemPresetCommand { UserId = user.Id, UserItemPresetId = preset.Id }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(AssertDb.UserItemPresets.Any(p => p.Id == preset.Id), Is.False);
    }
}
