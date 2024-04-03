using ChinookDataAccess.ClientModels;
using ChinookDataAccess.Contexts;
using ChinookDataAccess.Models;
using ChinookDataAccess.PageData.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChinookDataAccess.PageData
{
    public class IndexPageData : IIndexData
    {
        private readonly IDbContextFactory<ChinookContext> _dbContextFactory;

        public IndexPageData(IDbContextFactory<ChinookContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<ArtistClient>> GetArtists()
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var users = dbContext.Users.Include(a => a.UserPlaylists).ToList();

            return mapClientArtist(dbContext.Artists.ToList());
        }

        public async Task<List<Album>> GetAlbumsForArtist(int artistId)
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            return dbContext.Albums.Where(a => a.ArtistId == artistId).ToList();
        }

        #region mappers
        private List<ClientModels.ArtistClient> mapClientArtist(List<Models.Artist> modelArtist)
        {
            var newArtistClientList= new List<ClientModels.ArtistClient>();
            foreach (var artist in modelArtist)
            {
                var artistClient = new ArtistClient()
                {
                    Albums = artist.Albums,
                    ArtistId = artist.ArtistId,
                    Name = artist.Name
                };
                newArtistClientList.Add(artistClient);
            }
          
            return newArtistClientList;
        }
        #endregion
    }
}
