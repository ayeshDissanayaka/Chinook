using ChinookDataAccess.ClientModels;
using ChinookDataAccess.Models;

namespace ChinookDataAccess.PageData.Interfaces
{
    public interface IIndexData
    {
        Task<List<Album>> GetAlbumsForArtist(int artistId);
        Task<List<ArtistClient>> GetArtists();
    }
}