using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecuritySystem.Data;

namespace SecuritySystem.Business.Repositories
{
    public class PictureRepository
    {
        private SecuritySystemEntities _dbContext;
        private Encryption.EncryptorPicture _encrytor;
        public PictureRepository()
        {
            _dbContext = new SecuritySystemEntities();
            _encrytor = new Encryption.EncryptorPicture();
        }

        public List<Picture> GetPicturesByUserId(int userId){
            var pictures = _dbContext.Pictures.Where(x => x.UserId == userId).ToList();
            return pictures;
        }
    }
}
