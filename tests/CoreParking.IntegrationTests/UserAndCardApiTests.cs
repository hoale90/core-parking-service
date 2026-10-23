using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CoreParking.Api.Controllers;
using CoreParking.Application.Common.Models;
using CoreParking.Application.Features.Cards.Commands;
using CoreParking.Application.Features.Cards.DTOs;
using CoreParking.Application.Features.Users.DTOs;
using CoreParking.Domain.Entities;
using CoreParking.Domain.Enums;
using CoreParking.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoreParking.IntegrationTests;

public class UserAndCardApiTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ParkingDbContext> _options;

    // Seed Data Identifiers
    private readonly Guid _user1Id = Guid.Parse("E5C46E01-3850-48E2-9EBC-2B66236B28B1");
    private readonly Guid _user2Id = Guid.Parse("7B275BF9-5E58-45B6-BA56-9A2029BFBA90");
    private readonly Guid _user3Id = Guid.Parse("8C078021-E0E9-4E44-8629-CFA094DCA77F");

    private readonly Guid _card1Id = Guid.Parse("3FA85F64-5717-4562-B3FC-2C963F66AFA6");
    private readonly Guid _card2Id = Guid.Parse("12345678-ABCD-EF01-2345-6789ABCDEF01");
    private readonly Guid _card3Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
    private readonly Guid _cardLockedId = Guid.Parse("F1E2D3C4-B5A6-0987-6543-210FEDCBA987");

    public UserAndCardApiTests()
    {
        // 1. Táº¡o káº¿t ná»‘i SQLite In-Memory tháº­t vÃ  má»Ÿ káº¿t ná»‘i suá»‘t vÃ²ng Ä‘á»i test
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<ParkingDbContext>()
            .UseSqlite(_connection)
            .Options;

        // 2. Táº¡o schema DB tá»« EF Core Configurations bao gá»“m Filtered Unique Index
        using var context = new ParkingDbContext(_options);
        context.Database.EnsureCreated();

        // 3. Náº¡p dá»¯ liá»‡u kiá»ƒm thá»­ thá»±c táº¿
        SeedDatabase(context);
    }

    private void SeedDatabase(ParkingDbContext context)
    {
        var u1 = new User(_user1Id, "Nguyá»…n VÄƒn An", "0901234567", "29A-888.88", "12A-04");
        var u2 = new User(_user2Id, "Tráº§n Thá»‹ BÃ­ch", "0987654321", "59B1-123.45", "08B-12");
        var u3 = new User(_user3Id, "LÃª HoÃ ng Nam", "0911223344", "30E-999.99", "15C-01");
        context.Users.AddRange(u1, u2, u3);

        var c1 = new Card(_card1Id, "CARD-001", CardType.Resident, CardStatus.Active);
        c1.AssignToUser(_user1Id); // Card 1 gÃ¡n cho User 1

        var c2 = new Card(_card2Id, "CARD-002", CardType.Resident, CardStatus.Active); // Card 2 ráº£nh

        var c3 = new Card(_card3Id, "CARD-003", CardType.Resident, CardStatus.Active); // Card 3 ráº£nh

        var cLocked = new Card(_cardLockedId, "CARD-LOCKED", CardType.Resident, CardStatus.Locked); // Card khÃ³a

        context.Cards.AddRange(c1, c2, c3, cLocked);
        context.SaveChanges();
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    // =========================================================================
    // TEST 1: PhÃ¢n trang vÃ  tÃ¬m kiáº¿m cÆ° dÃ¢n theo nhiá»u trÆ°á»ng
    // =========================================================================
    [Fact]
    public async Task Test1_GetUsers_PaginationAndSearch_ShouldReturnCorrectPageAndCount()
    {
        using var context = new ParkingDbContext(_options);
        var controller = new UsersController(context);

        // Act 1: TÃ¬m kiáº¿m theo biá»ƒn sá»‘ xe "29A"
        var searchResult = await controller.GetUsers(pageNumber: 1, pageSize: 10, searchTerm: "29A") as OkObjectResult;
        searchResult.Should().NotBeNull();
        var apiRes1 = searchResult!.Value as ApiResponse<PaginatedResult<UserDto>>;
        apiRes1.Should().NotBeNull();
        apiRes1!.Success.Should().BeTrue();
        apiRes1.Data!.Items.Should().ContainSingle();
        apiRes1.Data.Items[0].FullName.Should().Be("Nguyá»…n VÄƒn An");
        apiRes1.Data.Items[0].LinkedCardNumber.Should().Be("CARD-001");

        // Act 2: PhÃ¢n trang pageSize = 2
        var pagedResult = await controller.GetUsers(pageNumber: 1, pageSize: 2) as OkObjectResult;
        pagedResult.Should().NotBeNull();
        var apiRes2 = pagedResult!.Value as ApiResponse<PaginatedResult<UserDto>>;
        apiRes2.Should().NotBeNull();
        apiRes2!.Data!.TotalCount.Should().Be(3);
        apiRes2.Data.Items.Should().HaveCount(2);
        apiRes2.Data.TotalPages.Should().Be(2);
        apiRes2.Data.HasNextPage.Should().BeTrue();
    }

    // =========================================================================
    // TEST 2: Lá»c cÆ° dÃ¢n theo tráº¡ng thÃ¡i cÃ³ tháº» / chÆ°a cÃ³ tháº»
    // =========================================================================
    [Fact]
    public async Task Test2_GetUsers_FilterByCardStatus_ShouldFilterAssignedAndUnassigned()
    {
        using var context = new ParkingDbContext(_options);
        var controller = new UsersController(context);

        // Act 1: Lá»c ngÆ°á»i chÆ°a cÃ³ tháº» (unassigned / no-card)
        var unassignedRes = await controller.GetUsers(status: "unassigned") as OkObjectResult;
        var unassignedData = (unassignedRes!.Value as ApiResponse<PaginatedResult<UserDto>>)!;
        unassignedData.Data!.TotalCount.Should().Be(2); // User 2 vÃ  User 3 chÆ°a cÃ³ tháº»
        unassignedData.Data.Items.Should().OnlyContain(u => u.LinkedCardId == null);

        // Act 2: Lá»c ngÆ°á»i Ä‘Ã£ cÃ³ tháº» (assigned / has-card)
        var assignedRes = await controller.GetUsers(status: "assigned") as OkObjectResult;
        var assignedData = (assignedRes!.Value as ApiResponse<PaginatedResult<UserDto>>)!;
        assignedData.Data!.TotalCount.Should().Be(1); // Chá»‰ User 1 Ä‘Ã£ cÃ³ tháº»
        assignedData.Data.Items[0].FullName.Should().Be("Nguyá»…n VÄƒn An");
        assignedData.Data.Items[0].LinkedCardNumber.Should().Be("CARD-001");
    }

    // =========================================================================
    // TEST 3: PhÃ¢n trang vÃ  lá»c tháº» theo tráº¡ng thÃ¡i ráº£nh / Ä‘Ã£ gÃ¡n / khÃ³a
    // =========================================================================
    [Fact]
    public async Task Test3_GetCards_PaginationAndStatusFilter_ShouldFilterAvailableAndLocked()
    {
        using var context = new ParkingDbContext(_options);
        var controller = new CardsController(context);

        // Act 1: Lá»c tháº» ráº£nh (available / unassigned)
        var availableRes = await controller.GetCards(status: "available") as OkObjectResult;
        var availableData = (availableRes!.Value as ApiResponse<PaginatedResult<CardDto>>)!;
        availableData.Data!.TotalCount.Should().Be(2); // Card 2, Card 3
        availableData.Data.Items.Should().OnlyContain(c => c.UserId == null && c.Status == (int)CardStatus.Active);

        // Act 2: Lá»c tháº» Ä‘Ã£ gÃ¡n (assigned)
        var assignedRes = await controller.GetCards(status: "assigned") as OkObjectResult;
        var assignedData = (assignedRes!.Value as ApiResponse<PaginatedResult<CardDto>>)!;
        assignedData.Data!.TotalCount.Should().Be(1);
        assignedData.Data.Items[0].CardNumber.Should().Be("CARD-001");
        assignedData.Data.Items[0].UserFullName.Should().Be("Nguyá»…n VÄƒn An");

        // Act 3: Lá»c tháº» bá»‹ khÃ³a (locked)
        var lockedRes = await controller.GetCards(status: "locked") as OkObjectResult;
        var lockedData = (lockedRes!.Value as ApiResponse<PaginatedResult<CardDto>>)!;
        lockedData.Data!.TotalCount.Should().Be(1);
        lockedData.Data.Items[0].CardNumber.Should().Be("CARD-LOCKED");
    }

    // =========================================================================
    // TEST 4: GÃ¡n tháº» ráº£nh há»£p lá»‡ cho cÆ° dÃ¢n thÃ nh cÃ´ng vÃ  lÆ°u vÃ o SQLite
    // =========================================================================
    [Fact]
    public async Task Test4_AssignUser_WhenCardAndUserValid_ShouldSucceedAndPersist()
    {
        using var context = new ParkingDbContext(_options);
        var controller = new CardsController(context);

        // GÃ¡n Card 2 (Ä‘ang ráº£nh) cho User 2 (chÆ°a cÃ³ tháº»)
        var request = new AssignUserRequest { UserId = _user2Id };
        var result = await controller.AssignUser(_card2Id, request) as OkObjectResult;

        result.Should().NotBeNull();
        var apiRes = result!.Value as ApiResponse<object?>;
        apiRes!.Success.Should().BeTrue();

        // XÃ¡c minh trá»±c tiáº¿p trong cÆ¡ sá»Ÿ dá»¯ liá»‡u SQLite
        var persistedCard = await context.Cards.AsNoTracking().FirstAsync(c => c.Id == _card2Id);
        persistedCard.UserId.Should().Be(_user2Id);
    }

    // =========================================================================
    // TEST 5: Cháº·n gÃ¡n tháº» Ä‘Ã£ thuá»™c vá» ngÆ°á»i khÃ¡c, tráº£ vá» 409 Conflict
    // =========================================================================
    [Fact]
    public async Task Test5_AssignUser_WhenCardAlreadyAssignedToAnotherUser_ShouldReturnConflict()
    {
        using var context = new ParkingDbContext(_options);
        var controller = new CardsController(context);

        // Cá»‘ tÃ¬nh gÃ¡n Card 1 (Ä‘ang thuá»™c User 1) cho User 2
        var request = new AssignUserRequest { UserId = _user2Id };
        var result = await controller.AssignUser(_card1Id, request) as ConflictObjectResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be((int)HttpStatusCode.Conflict);

        var apiRes = result.Value as ApiResponse<object?>;
        apiRes!.Success.Should().BeFalse();
        apiRes.Error!.Code.Should().Be("CARD_ALREADY_ASSIGNED");
        apiRes.Error.Message.Should().Contain("Nguyá»…n VÄƒn An");
    }

    // =========================================================================
    // TEST 6: Báº¥t biáº¿n 1 cÆ° dÃ¢n - 1 tháº»: Cáº¥p tháº» má»›i tá»± Ä‘á»™ng thu há»“i tháº» cÅ© qua Two-Phase Save
    // =========================================================================
    [Fact]
    public async Task Test6_AssignUser_WhenUserAlreadyHasAnotherCard_ShouldAutoReleaseOldCard()
    {
        using var context = new ParkingDbContext(_options);
        var controller = new CardsController(context);

        // User 1 hiá»‡n Ä‘ang giá»¯ Card 1. Cáº¥p Card 2 cho User 1 (Äá»•i tháº»)
        var request = new AssignUserRequest { UserId = _user1Id };
        var result = await controller.AssignUser(_card2Id, request) as OkObjectResult;

        result.Should().NotBeNull();

        // Kiá»ƒm tra trá»±c tiáº¿p trong DB
        var oldCard = await context.Cards.AsNoTracking().FirstAsync(c => c.Id == _card1Id);
        var newCard = await context.Cards.AsNoTracking().FirstAsync(c => c.Id == _card2Id);

        oldCard.UserId.Should().BeNull("Tháº» cÅ© pháº£i tá»± Ä‘á»™ng Ä‘Æ°á»£c thu há»“i vá» kho");
        newCard.UserId.Should().Be(_user1Id, "Tháº» má»›i pháº£i Ä‘Æ°á»£c gÃ¡n cho User 1");
    }

    // =========================================================================
    // TEST 7: Há»§y gÃ¡n tháº» (UserId = null) giáº£i phÃ³ng tháº» vá» kho ráº£nh
    // =========================================================================
    [Fact]
    public async Task Test7_AssignUser_UnassignWithNullUserId_ShouldReleaseCard()
    {
        using var context = new ParkingDbContext(_options);
        var controller = new CardsController(context);

        // Há»§y gÃ¡n Card 1 khá»i User 1
        var request = new AssignUserRequest { UserId = null };
        var result = await controller.AssignUser(_card1Id, request) as OkObjectResult;

        result.Should().NotBeNull();

        var card = await context.Cards.AsNoTracking().FirstAsync(c => c.Id == _card1Id);
        card.UserId.Should().BeNull();
    }

    // =========================================================================
    // TEST 8: Cáº­p nháº­t tráº¡ng thÃ¡i tháº» sang Lost/Locked tá»± Ä‘á»™ng giáº£i phÃ³ng UserId
    // =========================================================================
    [Fact]
    public async Task Test8_UpdateCardStatus_WhenLost_ShouldUnassignCardAndSetStatus()
    {
        using var context = new ParkingDbContext(_options);
        var controller = new CardsController(context);

        // BÃ¡o máº¥t Card 1 (Ä‘ang gÃ¡n cho User 1)
        var request = new UpdateCardStatusRequest { Status = (int)CardStatus.Lost };
        var result = await controller.UpdateCardStatus(_card1Id, request) as OkObjectResult;

        result.Should().NotBeNull();

        var card = await context.Cards.AsNoTracking().FirstAsync(c => c.Id == _card1Id);
        card.Status.Should().Be(CardStatus.Lost);
        card.UserId.Should().BeNull("Khi tháº» bá»‹ bÃ¡o máº¥t, UserId pháº£i tá»± Ä‘á»™ng Ä‘Æ°á»£c gá»¡ bá»");
    }

    // =========================================================================
    // TEST 9: Kiá»ƒm tra rÃ ng buá»™c váº­t lÃ½ Filtered Unique Index á»Ÿ má»©c DB SQLite
    // =========================================================================
    [Fact]
    public void Test9_DatabaseUniqueConstraint_DuplicateUserId_ShouldThrowDbUpdateException()
    {
        using var context = new ParkingDbContext(_options);

        // Cá»‘ tÃ¬nh vi pháº¡m chá»‰ má»¥c duy nháº¥t IX_Cards_UserId báº±ng cÃ¡ch gÃ¡n cÃ¹ng 1 UserId cho 2 tháº» khÃ¡c nhau
        var cardA = new Card(Guid.NewGuid(), "CARD-TEST-A");
        cardA.AssignToUser(_user3Id);

        var cardB = new Card(Guid.NewGuid(), "CARD-TEST-B");
        cardB.AssignToUser(_user3Id);

        context.Cards.AddRange(cardA, cardB);

        // Kháº³ng Ä‘á»‹nh SQLite nÃ©m DbUpdateException khi vi pháº¡m rÃ ng buá»™c UNIQUE
        var act = () => context.SaveChanges();
        act.Should().Throw<DbUpdateException>()
            .WithInnerException<SqliteException>()
            .WithMessage("*UNIQUE constraint failed: Cards.UserId*");
    }

    // =========================================================================
    // TEST 10: Kiá»ƒm tra so khá»›p GUID khÃ´ng phÃ¢n biá»‡t hoa thÆ°á»ng (UPPERCASE & NOCASE)
    // =========================================================================
    [Fact]
    public async Task Test10_GuidCaseSensitivity_WhenQueryingWithUppercase_ShouldMatch()
    {
        using var context = new ParkingDbContext(_options);
        var controller = new UsersController(context);

        // Truy váº¥n báº±ng GUID in hoa
        var uppercaseGuid = _user1Id.ToString().ToUpperInvariant();
        var resultUpper = await controller.GetUserById(Guid.Parse(uppercaseGuid)) as OkObjectResult;
        resultUpper.Should().NotBeNull();

        // Truy váº¥n báº±ng GUID in thÆ°á»ng
        var lowercaseGuid = _user1Id.ToString().ToLowerInvariant();
        var resultLower = await controller.GetUserById(Guid.Parse(lowercaseGuid)) as OkObjectResult;
        resultLower.Should().NotBeNull();

        var userUpper = ((ApiResponse<UserDto>)resultUpper!.Value!).Data;
        var userLower = ((ApiResponse<UserDto>)resultLower!.Value!).Data;

        userUpper!.Id.Should().Be(userLower!.Id);
        userUpper.FullName.Should().Be("Nguyá»…n VÄƒn An");
    }

    // =========================================================================
    // TEST 11: Xá»­ lÃ½ tranh cháº¥p Ä‘á»“ng thá»i khi lÆ°u DB tráº£ vá» 409 CONCURRENT_CONFLICT
    // =========================================================================
    [Fact]
    public async Task Test11_AssignUser_WhenConcurrentConflict_ShouldReturn409Conflict()
    {
        using var context = new ParkingDbContext(_options);
        var controller = new CardsController(context);

        // Hook SavingChanges event to simulate concurrent conflict:
        // Right before Phase 2 saves changes, another transaction directly updates SQLite to assign another card to _user2Id
        context.SavingChanges += (sender, args) =>
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = $"UPDATE Cards SET UserId = '{_user2Id.ToString().ToUpperInvariant()}' WHERE Id = '{_card1Id.ToString().ToUpperInvariant()}';";
            cmd.ExecuteNonQuery();
        };

        // Act: Attempt to assign Card 2 to User 2 -> triggers DbUpdateException due to IX_Cards_UserId unique constraint
        var request = new AssignUserRequest { UserId = _user2Id };
        var result = await controller.AssignUser(_card2Id, request) as ConflictObjectResult;

        // Assert: Real assertion that controller catches DbUpdateException and returns HTTP 409 ConflictObjectResult
        result.Should().NotBeNull("AssignUser must catch DbUpdateException and return 409 ConflictObjectResult");
        result!.StatusCode.Should().Be((int)HttpStatusCode.Conflict);

        var apiRes = result.Value as ApiResponse<object?>;
        apiRes.Should().NotBeNull();
        apiRes!.Success.Should().BeFalse();
        apiRes.Error!.Code.Should().Be("CONCURRENT_CONFLICT");
        apiRes.Error.Message.Should().Be("Tháº» hoáº·c cÆ° dÃ¢n Ä‘Ã£ bá»‹ gÃ¡n trong phiÃªn lÃ m viá»‡c khÃ¡c");
    }
}