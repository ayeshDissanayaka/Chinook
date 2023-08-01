using ChinookDataAccess.Contexts;
using ChinookDataAccess.Models;
using ChinookDataAccess.Pages.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChinookDataAccess.Pages
{
    public class IndexPageData : IIndexData
    {
        private readonly IDbContextFactory<ChinookContext> _dbContextFactory;

        public IndexPageData(IDbContextFactory<ChinookContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Artist>> GetArtists()
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var users = dbContext.Users.Include(a => a.UserPlaylists).ToList();

            return dbContext.Artists.ToList();
        }

        public async Task<List<Album>> GetAlbumsForArtist(int artistId)
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            return dbContext.Albums.Where(a => a.ArtistId == artistId).ToList();
        }
    }
}
