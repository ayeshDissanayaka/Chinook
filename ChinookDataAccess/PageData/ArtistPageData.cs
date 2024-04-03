using ChinookDataAccess.ClientModels;
using ChinookDataAccess.Contexts;
using ChinookDataAccess.Models;
using ChinookDataAccess.PageData.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChinookDataAccess.PageData
{
    public class ArtistPageData : IArtistPage
    {

        private readonly IDbContextFactory<ChinookContext> _dbContextFactory;

        public ArtistPageData(IDbContextFactory<ChinookContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
        public async Task<List<PlaylistTrackClient?>> getTrackList(int ArtistId,string CurrentUserId)
        {
            var DbContext = await _dbContextFactory.CreateDbContextAsync();
            var Tracks = DbContext.Tracks.Where(a => a.Album.ArtistId == ArtistId)
            .Include(a => a.Album)
            .Select(t => new PlaylistTrackClient()
            {
                AlbumTitle = (t.Album == null ? "-" : t.Album.Title),
                TrackId = t.TrackId,
                TrackName = t.Name,
                IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == CurrentUserId && up.PlaylistId == 19)).Any()
            })
            .ToList();

            return Tracks;
        }
        public async Task<ClientModels.ArtistClient?>getArtistById(int artistId)
        {
            var DbContext = await _dbContextFactory.CreateDbContextAsync();
            return mapClientArtist(DbContext.Artists.SingleOrDefault(a => a.ArtistId == artistId));
        }
        #region mappers
        private ClientModels.ArtistClient mapClientArtist(Models.Artist modelArtist)
        {

                var artistClient = new ArtistClient()
                {
                    Albums = modelArtist.Albums,
                    ArtistId = modelArtist.ArtistId,
                    Name = modelArtist.Name
                };

            return artistClient;
        }
        #endregion
    }

}

