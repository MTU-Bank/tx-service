using Microsoft.EntityFrameworkCore;
using MTUModelContainer.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTUTxService
{
    public class AccountManager
    {
        public static async Task<List<Account>> FindAccounts(User user)
        {
            using (var db = new ApplicationContext())
            {
                var accounts = db.Accounts.Where((z) => z.OwnerId == user.Id);
                return await accounts.ToListAsync();
            }
        }

        public static async Task<Account?> FindAccountById(string id)
        {
            using (var db = new ApplicationContext())
            {
                var accounts = db.Accounts.Where((z) => z.AccountId == id);
                return await accounts.FirstOrDefaultAsync();
            }
        }

        public static async Task<Account?> FindDefaultAccount(string phoneNum)
        {
            using (var db = new ApplicationContext())
            {
                var user = db.Users.Where((z) => z.PhoneNum == phoneNum).FirstOrDefault();
                if (user is null) return null;
                var accounts = db.Accounts.Where((z) => z.OwnerId == user.Id && z.IsDefault);
                return await accounts.FirstOrDefaultAsync();
            }
        }
    }
}
