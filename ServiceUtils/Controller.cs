using EmbedIO.Routing;
using EmbedIO.WebApi;
using EmbedIO;
using MTUModelContainer.Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTUModelContainer.Transactions.Models;
using MTUBankBase.ServiceManager;
using MTUAuthService.AuthService;
using MTUModelContainer.Database.Models;
using MTUModelContainer.SharedModels;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace MTUTxService.ServiceUtils
{
    internal class Controller : ServiceStub
    {
        [Route(HttpVerbs.Get, "/api/listAccounts")]
        [RequiresAuth]
        public async Task<AccountListResponse> ListAccounts()
        {
            // attempting to return the account list
            try
            {
                // find all accounts
                var accounts = await AccountManager.FindAccounts(HttpContext.CurrentUser);

                var alr = new AccountListResponse();
                alr.Accounts = accounts;

                return alr;
            }
            catch (Exception ex)
            {
                return new AccountListResponse() { Success = false, Error = ex.Message };
            }
        }

        [Route(HttpVerbs.Post, "/api/createAccount")]
        [RequiresAuth]
        public async Task<AccountResponse> CreateAccount([JsonData] AccountCreationRequest request)
        {
            // find all accounts
            var accounts = await AccountManager.FindAccounts(HttpContext.CurrentUser);
            // limit to 10 accounts
            if (accounts.Count > 10)
            {
                return new AccountResponse() { Success = false, Error = "Account count limit achieved" };
            }

            // create the account
            Account newAcc = null;
            using (var db = new ApplicationContext())
            {
                newAcc = new Account()
                {
                    AccountCurrency = request.Currency,
                    AccountId = Guid.NewGuid().ToString(),
                    Balance = 0,
                    CreationDate = DateTime.UtcNow,
                    FriendlyName = request.AccountName,
                    SystemLocked = false,
                    OwnerId = HttpContext.CurrentUser.Id,
                    UserLocked = false
                };

                db.Accounts.Add(newAcc);
                await db.SaveChangesAsync();
            }

            var resp = new AccountResponse(newAcc)
            {
                Success = true
            };
            return resp;
        }

        [Route(HttpVerbs.Post, "/api/deleteAccount")]
        [RequiresAuth]
        public async Task<SuccessResponse> DeleteAccount([JsonData] AccountRequest request)
        {
            // find all accounts
            var accounts = await AccountManager.FindAccounts(HttpContext.CurrentUser);

            // find account up for removal
            var toRemove = accounts.FirstOrDefault(x => x.AccountId == request.AccountId);

            if (toRemove is null)
            {
                return new SuccessResponse() { Success = false, Error = "This account doesn't exist!" };
            }
            
            using (var db = new ApplicationContext())
            {
                db.Accounts.Remove(toRemove);
                await db.SaveChangesAsync();
            }
            return new SuccessResponse() { Success = true, Error = "Successfully removed!" };
        }

        [Route(HttpVerbs.Post, "/api/getAccount")]
        [RequiresAuth]
        public async Task<AccountResponse> GetAccount([JsonData] AccountRequest request)
        {
            // find all accounts
            var accounts = await AccountManager.FindAccounts(HttpContext.CurrentUser);

            // find account up for display
            var requested = accounts.FirstOrDefault(x => x.AccountId == request.AccountId);

            if (requested is null)
            {
                return new AccountResponse() { Success = false, Error = "This account doesn't exist!" };
            }
            return new AccountResponse(requested) { Success = true };
        }

        [Route(HttpVerbs.Post, "/api/blockAccount")]
        [RequiresAuth]
        public async Task<SuccessResponse> BlockAccount([JsonData] AccountRequest request)
        {
            // find all accounts
            var accounts = await AccountManager.FindAccounts(HttpContext.CurrentUser);

            // find account up for block/unblock
            var requested = accounts.FirstOrDefault(x => x.AccountId == request.AccountId);

            if (requested is null)
            {
                return new SuccessResponse() { Success = false, Error = "This account doesn't exist!" };
            }

            using (var db = new ApplicationContext())
            {
                if (requested.UserLocked) requested.UserLocked = false;
                else requested.UserLocked = true;

                db.Update(requested);
                await db.SaveChangesAsync();
            }

            return new SuccessResponse() { Success = true };
        }

        [Route(HttpVerbs.Post, "/api/lookupAccount")]
        [RequiresAuth]
        public async Task<SuccessResponse> LookupAccount([JsonData] AccountLookupRequest request)
        {
            // find the account by request
            var account = await AccountManager.FindDefaultAccount(request.PhoneNum);

            if (account is null)
            {
                return new SuccessResponse() { Success = false, Error = "No account was found!" };
            }
            return new SuccessResponse() { Success = true };
        }

        [Route(HttpVerbs.Post, "/api/transferFunds")]
        [RequiresAuth]
        public async Task<SuccessResponse> TransferFunds([JsonData] TransactionRequest request)
        {
            // find the account by request
            Account recepient = null;
            if (request.IsDirectAccountTx)
            {
                recepient = await AccountManager.FindAccountById(request.ToWhoever);
            }
            else
            {
                recepient = await AccountManager.FindDefaultAccount(request.ToWhoever);
            }

            if (recepient is null)
            {
                return new SuccessResponse() { Success = false, Error = "No recepient account was found!" };
            }

            // find the sender account
            var sender = await AccountManager.FindAccountById(request.FromAccount);
            
            // ensure sender is user's account
            if (sender.OwnerId != HttpContext.CurrentUser.Id)
            {
                return new SuccessResponse() { Success = false, Error = "No access to this account!" };
            }
            // ensure the sender has enough funds
            if (sender.Balance <= request.Amount)
            {
                return new SuccessResponse() { Success = false, Error = "Insufficient funds!" };
            }
            // ensure accounts are different
            if (sender.AccountId == recepient.AccountId)
            {
                return new SuccessResponse() { Success = false, Error = "Cannot send money to yourself!" };
            }
            // ensure account currencies are EQUAL
            if (sender.AccountCurrency != recepient.AccountCurrency)
            {
                return new SuccessResponse() { Success = false, Error = "You can only send money to the account of the same currency." };
            }
            // ensure amount is good
            if (!(request.Amount > 0))
            {
                return new SuccessResponse() { Success = false, Error = "You can only send a positive amount of money." };
            }

            // proceed with the transaction
            using (var db = new ApplicationContext())
            {
                sender.Balance -= request.Amount;
                recepient.Balance += request.Amount;

                db.Update(sender);
                db.Update(recepient);
                await db.SaveChangesAsync();
            }

            return new SuccessResponse() { Success = true };
        }

        [Route(HttpVerbs.Post, "/api/setAsDefault")]
        [RequiresAuth]
        public async Task<SuccessResponse> SetAsDefaultAccount([JsonData] AccountRequest req)
        {
            // find all accounts
            var accounts = await AccountManager.FindAccounts(HttpContext.CurrentUser);

            // find the account requested
            var requested = accounts.FirstOrDefault(x => x.AccountId == req.AccountId);

            if (requested is null)
            {
                return new SuccessResponse() { Success = false, Error = "This account doesn't exist!" };
            }

            // set all other accounts as NON-DEFAULT, account requested as DEFAULT
            using (var db = new ApplicationContext())
            {
                await db.Accounts.ForEachAsync((z) =>
                {
                    if (z.OwnerId != HttpContext.CurrentUser.Id) return;
                    z.IsDefault = false;
                    if (z.AccountId == requested.AccountId) z.IsDefault = true;
                });
                await db.SaveChangesAsync();
            }
            return new SuccessResponse() { Success = true };
        }
    }
}
