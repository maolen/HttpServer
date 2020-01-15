using System;
using System.Collections.Generic;
using static BCrypt.Net.BCrypt;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace HttpWebServer
{
     public class AuthService
    {

        public async Task<bool> SignUp(User user)
        {
            using var databaseContext = new DatabaseContext();
            var userDb = databaseContext.Users.FirstOrDefault(x => x.Login == user.Login);
            if(userDb != null)
            {
                return false;
            }
            else
            {
                user.Password = HashPassword(user.Password);
                await databaseContext.Users.AddAsync(user);
                await databaseContext.SaveChangesAsync();
                return true;
            }
        }

        public User Auth(User user)
        {
            using var databaseContext = new DatabaseContext();
            var userDb = databaseContext.Users.SingleOrDefault(x => x.Login == user.Login);
            if (user is null || !Verify(user.Password, userDb.Password))
            {
                return null;
            }
            return user;
        }
    }
}
