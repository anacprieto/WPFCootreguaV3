
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace DB
{
    public static class DB_TransactionService
    {
        
        static DB_TransactionService()
        {

        }

        public static async Task<List<DB_Transaction>> GetAll() 
        {
            return await Task.Run(() =>
            {
                using (var db = new LocalContext())
                {
                    return db.DB_Transactions.ToList();
                }
            });
            
        }

        public static async Task<List<DB_Transaction>> GetByState(int state)
        {
            return await Task.Run(() =>
            {
                using (var db = new LocalContext())
                {
                    return db.DB_Transactions.Where(x => x.IdStateTransaction == state).ToList();
                }
            });
        }

        public static async Task<List<DB_Transaction>> GetByReference(string reference) 
        {
            return await Task.Run(() =>
            {
                using (var db = new LocalContext())
                {
                    return db.DB_Transactions.Where(x => x.Reference == reference).ToList();
                }
            });
        }

        public static async Task<bool> Create(DB_Transaction transaction)
        {
           return await Task.Run(() =>
            {
                using (var db = new LocalContext())
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        transaction.DateCreated = DateTime.Now;
                        db.DB_Transactions.Add(transaction);
                        db.SaveChanges();
                        tran.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        
                        tran.Rollback();
                        return false;
                    }
                    
                }
            });
        }

        public static async Task<bool> Update(DB_Transaction transaction)
        {
            return await Task.Run(() =>
            {
                using (var db = new LocalContext())
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var localTran = db.DB_Transactions.Where(ent => ent.IdApi ==  transaction.IdApi).First();
                        if (localTran == null) 
                            throw new Exception($"Registro con IdApi {transaction.IdApi} no se encontró");
                        //Se tienen que actualizar todos los campos

                        localTran.Document = transaction.Document;
                        localTran.Reference = transaction.Reference;
                        localTran.Product = transaction.Product;
                        localTran.TotalAmount = transaction.TotalAmount;
                        localTran.ReturnAmount = transaction.ReturnAmount;
                        localTran.IncomeAmount = transaction.IncomeAmount;
                        localTran.RealAmount = transaction.RealAmount;
                        localTran.Description = transaction.Description;
                        localTran.IdStateTransaction = transaction.IdStateTransaction;
                        localTran.StateTransaction = transaction.StateTransaction;
                        localTran.DateCreated = transaction.DateCreated;
                        localTran.DateUpdated = transaction.DateUpdated;
                        
                        db.SaveChanges();
                        tran.Commit();
                        return true;
                    }
                    catch (Exception)
                    {

                        tran.Rollback();
                        return false;
                    }

                }
            });
        }

        public static async Task<bool> CreateDetail(DB_TransactionDetail transactionDetail)
        {
            return await Task.Run(() =>
            {
                using (var db = new LocalContext())
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        transactionDetail.DateCreated = DateTime.Now;
                        db.DB_TransactionDetails.Add(transactionDetail);
                        db.SaveChanges();
                        tran.Commit();
                        return true;
                    }
                    catch (Exception)
                    {

                        tran.Rollback();
                        return false;
                    }

                }
            });
        }

    }
}
