using ChinookDataAccess.Models;

namespace ChinookDataAccess.Pages.Interfaces
{
    public interface IIndexData
    {
        Task<List<Album>> GetAlbumsForArtist(int artistId);
        Task<List<Artist>> GetArtists();
    }
}