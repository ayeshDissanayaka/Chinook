using ChinookDataAccess.ClientModels;
using ChinookDataAccess.Models;

namespace ChinookDataAccess.Pages.Interfaces
{
    public interface IArtistPage
    {
        Task<Artist?> getArtistById(int artistId);
        Task<List<PlaylistTrack?>> getTrackList(int ArtistId, string CurrentUserId);
    }
}