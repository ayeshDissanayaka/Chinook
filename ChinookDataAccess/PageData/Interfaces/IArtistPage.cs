using ChinookDataAccess.ClientModels;
using ChinookDataAccess.Models;
using ArtistClient = ChinookDataAccess.ClientModels.ArtistClient;

namespace ChinookDataAccess.PageData.Interfaces
{
    public interface IArtistPage
    {
        Task<ArtistClient?> getArtistById(int artistId);
        Task<List<PlaylistTrackClient?>> getTrackList(int ArtistId, string CurrentUserId);
    }
}