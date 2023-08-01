using ChinookDataAccess.Models;

namespace ChinookDataAccess.Pages.Interfaces
{
    public interface IPlayListPageData
    {
        Task<Models.Playlist> getPlayListById(long playListId);
        Task<List<UserPlaylist>> getPlayListByUser(string userId);
        Task<ClientModels.Playlist> getPlayListDetailsByUserId(string userId, long PlaylistId);
        Task<string> postTracksForPlayList(string playListName, long trackId,string userName);
    }
}