using Microsoft.VisualStudio.TestTools.UnitTesting;
using GuguEveryday.Models;
using GuguEveryday.Models.Dtos;
using System.Text.Json;

namespace GuguEveryday.Tests;

[TestClass]
public class GuguMapperTests
{
    private static T WithId<T>(T entity, long id) where T : class
    {
        typeof(BaseModel).GetProperty(nameof(BaseModel.Id))!.SetValue(entity, id);
        return entity;
    }

    [TestMethod]
    public void UserInfo_ToDto_MapsAllProperties()
    {
        var entity = WithId(new UserInfo
        {
            UserId = 100,
            UserName = "测试用户",
            Bulletin = "这是公告",
            IsShowPageEnabled = true,
            ShowPageTitle = "我的项目",
            IsAllowReminder = true,
            Mask = "*"
        }, 1);

        var dto = entity.ToDto();

        Assert.AreEqual(1, dto.Id);
        Assert.AreEqual(100, dto.UserId);
        Assert.AreEqual("测试用户", dto.UserName);
        Assert.AreEqual("这是公告", dto.Bulletin);
        Assert.IsTrue(dto.IsShowPageEnabled);
        Assert.AreEqual("我的项目", dto.ShowPageTitle);
        Assert.IsTrue(dto.IsAllowReminder);
        Assert.AreEqual("*", dto.Mask);
    }

    [TestMethod]
    public void CreateUpdateUserInfoDto_ToModel_MapsAllProperties()
    {
        var dto = new CreateUpdateUserInfoDto
        {
            UserName = "新名字",
            Bulletin = "新公告",
            IsShowPageEnabled = false,
            ShowPageTitle = "新标题",
            IsAllowReminder = false,
            Mask = "#"
        };

        var entity = dto.ToModel();

        Assert.AreEqual("新名字", entity.UserName);
        Assert.AreEqual("新公告", entity.Bulletin);
        Assert.IsFalse(entity.IsShowPageEnabled);
        Assert.AreEqual("新标题", entity.ShowPageTitle);
        Assert.IsFalse(entity.IsAllowReminder);
        Assert.AreEqual("#", entity.Mask);
    }

    [TestMethod]
    public void ProjectProgress_ToDto_MapsAllProperties()
    {
        var entity = WithId(new ProjectProgress
        {
            ProjectId = 5,
            CurrentProgress = "50",
            NextReportProgress = "60",
            CreationTime = new DateTime(2025, 1, 1)
        }, 10);

        var dto = entity.ToDto();

        Assert.AreEqual(10, dto.Id);
        Assert.AreEqual("50", dto.CurrentProgress);
        Assert.AreEqual("60", dto.NextReportProgress);
    }

    [TestMethod]
    public void Reminder_ToDto_MapsAllProperties()
    {
        var project = WithId(new Project
        {
            Name = "测试项目",
            UserId = 2,
            Order = 0,
            IsPublic = true
        }, 3);

        var entity = WithId(new Reminder
        {
            UserId = 2,
            ProjectId = 3,
            Project = project,
            Ip = "127.0.0.1",
            UserAgent = "TestAgent",
            CreationTime = new DateTime(2025, 1, 1)
        }, 1);

        var dto = entity.ToDto();

        Assert.AreEqual(3, dto.ProjectId);
    }

    [TestMethod]
    public void Project_WithEmptyExtraData_MapsToEmptyJsonElement()
    {
        var entity = WithId(new Project
        {
            Name = "测试项目",
            ExtraData = "{}"
        }, 1);

        var dto = entity.ToDto();

        Assert.AreEqual("{}", dto.ExtraData.GetRawText());
    }

    [TestMethod]
    public void Project_WithExtraData_MapsCorrectly()
    {
        var entity = WithId(new Project
        {
            Name = "测试项目",
            ExtraData = "{\"totalProgress\":\"100\",\"progressUnit\":\"秒\"}"
        }, 1);

        var dto = entity.ToDto();

        Assert.AreEqual("100", dto.ExtraData.GetProperty("totalProgress").GetString());
        Assert.AreEqual("秒", dto.ExtraData.GetProperty("progressUnit").GetString());
    }

    [TestMethod]
    public void ProjectType_WithStepExtraData_MapsCorrectly()
    {
        var entity = WithId(new ProjectType
        {
            Name = "绘画",
            ProgressType = Enums.ProgressType.Step,
            ExtraData = "{\"steps\":[\"草图\",\"线稿\",\"上色\"]}"
        }, 1);

        var dto = entity.ToDto();

        var steps = dto.ExtraData.GetProperty("steps");
        Assert.AreEqual(3, steps.GetArrayLength());
        Assert.AreEqual("草图", steps[0].GetString());
    }

    [TestMethod]
    public void Project_CollectionMapping_Works()
    {
        var entities = new List<Project>
        {
            WithId(new Project { Name = "项目1" }, 1),
            WithId(new Project { Name = "项目2" }, 2)
        };

        var dtos = ((ICollection<Project>)entities).ToDto();

        Assert.AreEqual(2, dtos.Count);
    }

    [TestMethod]
    public void CreateUpdateProjectProgressDto_ToModel_MapsCorrectly()
    {
        var dto = new CreateUpdateProjectProgressDto
        {
            CurrentProgress = "75",
            NextReportProgress = "80"
        };

        var entity = dto.ToModel();

        Assert.AreEqual("75", entity.CurrentProgress);
        Assert.AreEqual("80", entity.NextReportProgress);
    }
}
