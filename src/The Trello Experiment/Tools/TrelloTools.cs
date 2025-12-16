using System.ComponentModel;
using TrelloDotNet;
using TrelloDotNet.Model;
using TrelloDotNet.Model.Options;
using TrelloDotNet.Model.Options.AddCardOptions;
using TrelloDotNet.Model.Options.GetBoardOptions;
using TrelloDotNet.Model.Options.GetCardOptions;
using TrelloDotNet.Model.Options.GetListOptions;

namespace The_Trello_Experiment.Tools;

public class TrelloTools(TrelloClient trelloClient)
{
    public async Task<List<Board>> GetBoards()
    {
        return await trelloClient.GetBoardsCurrentTokenCanAccessAsync(new GetBoardOptions
        {
            BoardFields = new BoardFields(BoardFieldsType.Name, BoardFieldsType.ShortUrl, BoardFieldsType.Closed)
        });
    }

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

    public async Task<List<Label>> GetLabelsOnBoard(string boardId)
    {
        return await trelloClient.GetLabelsOfBoardAsync(boardId);
    }

    public async Task<Card> AddNewCard(AddCardOptions addCardOptions)
    {
        return await trelloClient.AddCardAsync(addCardOptions);
    }

    public async Task<Card> MoveCard(string cardId, string newListId)
    {
        return await trelloClient.MoveCardToListAsync(cardId, newListId);
    }
}