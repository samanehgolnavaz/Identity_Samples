using Dapper;
using Microsoft.AspNetCore.Identity;
using System.Data.Common;
using System.Data.SqlClient;

namespace Identity_Samples
{
    public class UserStore : IUserStore<User>,IUserPasswordStore<User>
    {

        public static DbConnection GetOpenConnection()
        {
            var connection = new SqlConnection(@"Data Source=LAPTOP-VAHA5KRU\sql_2022;User ID=sa;Password=sql;Encrypt=False;" +
                "database=Identity_Samples");
            connection.Open();
            return connection;
        }
        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            using (var connection = GetOpenConnection())
            {
                await connection.ExecuteAsync(
                   "INSERT INTO [dbo].[Users]([Id]," +
                   "[UserName]," +
                   "[NormalizedUserName]," +
                   "[PasswordHash])" +
                   "VALUES (@id,@userName,@normalizedUserName,@passwordHash)",
                   new
                   {
                       id = user.Id,
                       username = user.UserName,
                       normalizedUserName = user.NormalizedUserName,
                       passwordHash = user.PasswordHash
                   }
                  );
            }
            return IdentityResult.Success;
        }

        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }

        public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            using (var connection=GetOpenConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<User>
                    (
                    "select * from users where Id=@id", new { id = userId }
                    );
            }
        }

        public async Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            using (var connection = GetOpenConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<User>
                    (
                    "select * from users where [NormalizedUserName]=@normalizedUserName", new { normalizedUserName = normalizedUserName }
                    );
            }
        }

        public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

     

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }
        public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }
        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash != null);
        }
        public Task SetPasswordHashAsync(User user, string? passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetNormalizedUserNameAsync(User user, string? normalizedUserName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedUserName;
            return Task.CompletedTask;
        }

 

        public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            using (var connection = GetOpenConnection())
            {
                await connection.ExecuteAsync(
                   "UPDATE [dbo].[Users]" +
                   " SET[Id] = @id," +
                   "[UserName] = @userName," +
                   "[NormalizedUserName] = @normalizedUserName," +
                   "[PasswordHash] = @passwordHash" +
                   " WHERE[Id] = @id",
                   new
                   {
                       id = user.Id,
                       username = user.UserName,
                       normalizedUserName = user.NormalizedUserName,
                       passwordHash = user.PasswordHash
                   }
                  );
            }
            return IdentityResult.Success;
        }
    }
}
