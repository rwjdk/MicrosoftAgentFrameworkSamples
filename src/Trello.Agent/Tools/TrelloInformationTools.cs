using System.ComponentModel;
using TrelloDotNet;
using TrelloDotNet.Model;
using TrelloDotNet.Model.Options;
using TrelloDotNet.Model.Options.AddCardOptions;
using TrelloDotNet.Model.Options.GetBoardOptions;
using TrelloDotNet.Model.Options.GetCardOptions;
using TrelloDotNet.Model.Options.GetListOptions;

namespace Trello.Agent.Tools;

public class TrelloInformationTools(TrelloClient trelloClient)
{
    private string Private()
    {
        return "Hello";
    }

    private static string PrivateStatic()
    {
        return "Private Static";
    }

    public async Task<List<Board>> GetBoards()
    {
        return await trelloClient.GetBoardsCurrentTokenCanAccessAsync(new GetBoardOptions
        {
            BoardFields = new BoardFields(BoardFieldsType.Name, BoardFieldsType.ShortUrl, BoardFieldsType.Closed)
        });
    }

    [Description("Get the Lists on the board")]
    public async Task<List<List>> GetListsOnBoard(string boardId)
    {
        return await trelloClient.GetListsOnBoardAsync(boardId, new GetListOptions
        {
            ListFields = new ListFields(ListFieldsType.BoardId, ListFieldsType.Color, ListFieldsType.Name, ListFieldsType.Position, ListFieldsType.Closed)
        });
    }

    public async Task<List<Card>> GetCardsOnBoard(string boardId, GetCardOptions? getCardOptions)
    {
        return await trelloClient.GetCardsOnBoardAsync(boardId, getCardOptions);
    }

    public async Task<Card> AddNewCard(AddCardOptions addCardOptions)
    {
        return await trelloClient.AddCardAsync(addCardOptions);
    }

    public async Task<Card> UpdateCard(string cardId, List<CardUpdate> updates)
    {
        return await trelloClient.UpdateCardAsync(cardId, updates);
    }
}