using ChinookDataAccess.ClientModels;
using ChinookDataAccess.Models;

namespace ChinookDataAccess.PageData.Interfaces
{
    public interface IPlayListPageData
    {
        Task<List<UserPlaylistClient>?> getFilteredPlayListByUser(long trackId, string userId);
        Task<Models.Playlist> getPlayListById(long playListId);
        Task<List<UserPlaylistClient>?> getPlayListByUser(string userId);
        Task<ClientModels.Playlist?> getPlayListDetailsByUserId(string userId, long PlaylistId);
        Task<string> postTracksForPlayList(string playListName, long trackId,string userName);
        Task<string?> removeTrackFromPlayList(string playListName, long trackId, string? userId);
    }
}