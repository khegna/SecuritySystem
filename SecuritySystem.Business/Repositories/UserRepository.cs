using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecuritySystem.Data;

namespace SecuritySystem.Business.Repositories
{
    public class UserRepository
    {
        private SecuritySystemEntities _dbContext;
        private Encryption.Encryptor _encrytor;
        public UserRepository()
        {
            _dbContext = new SecuritySystemEntities();
            _encrytor = new Encryption.Encryptor();
        }

        public List<User> GetUsers()
        {
            var users = _dbContext.Users.ToList();
            return users;
        }

        public User GetUserById(int userId)
        {
            var user = _dbContext.Users.Where(x => x.UserId == userId).FirstOrDefault();
            return user;
        }

        public void CreateUser(User user)
        {
            user.UserPassword = _encrytor.Encrypt(user.UserPassword);
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }

        public void DeleteUser(int? id)
        {
            if(id.HasValue)
            {
                var user = GetUserById(id.Value);
                if(user != null)
                {
                    _dbContext.Users.Remove(user);
                    _dbContext.SaveChanges();
                }
            }
        }

        public void EditUser(User user)
        {
            var userEdit = GetUserById(user.UserId);
            userEdit.UserName = user.UserName;
            userEdit.LastName = user.LastName;
            userEdit.FirstName = user.FirstName;
            userEdit.Email = user.Email;
            userEdit.UserPassword = _encrytor.Encrypt(user.UserPassword);
            _dbContext.SaveChanges();
        }
    }
}
