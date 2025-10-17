using System.Data;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Extensions.Configuration.CommandLine;

namespace Grocery.Core.Data.Repositories
{
    public class GroceryListItemsRepository : DatabaseConnection, IGroceryListItemsRepository
    {
        private readonly List<GroceryListItem> groceryListItems;

        public GroceryListItemsRepository()
        {
            CreateTable(@"CREATE TABLE IF NOT EXISTS GroceryListItem (
                            [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            GroceryListId INTEGER NOT NULL,
                            ProductId INTEGER NOT NULL,
                            Quantity INTEGER NOT NULL);
            ");
        }

        public List<GroceryListItem> GetAll()
        {
            var groceryItems = new List<GroceryListItem>();
            OpenConnection();
            using (var command = Connection.CreateCommand())
            {
                Connection.CreateCommand().CommandText =
                    "Select Id, GroceryListId, ProductId, Quantity FROM GroceryListItem;";
                using (var reader = command.ExecuteReader())

                    while (reader.Read())
                    {
                        groceryItems.Add(new GroceryListItem(
                            reader.GetInt32(0),
                            reader.GetInt32(1),
                            reader.GetInt32(2),
                            reader.GetInt32(3)
                        ));
                    }
            }

            CloseConnection();
            return groceryItems;
        }

        public List<GroceryListItem> GetAllOnGroceryListId(int id)
        {
            var idList = new List<GroceryListItem>();
            OpenConnection();
            using (var command = Connection.CreateCommand())
            {
                Connection.CreateCommand().CommandText =
                    "SELECT Id, GroceryListId, ProductId, Quantitiy FROM GroceryListItem WHERE groceryListId = @id; ";
                command.Parameters.AddWithValue("@id", id);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        idList.Add(new GroceryListItem(
                            reader.GetInt32(0),
                            reader.GetInt32(1),
                            reader.GetInt32(2),
                            reader.GetInt32(3)
                        ));
                    }
                }
            }

            return groceryListItems.Where(g => g.GroceryListId == id).ToList();
        }

        public GroceryListItem Add(GroceryListItem item)
        {
            OpenConnection();
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"INSERT INTO GroceryListItem(GrocerListId, ProductId, Quantity) 
                                        VALUES (@id, @productId, @quantity
                                        SELECT last_insert_rowid();";
                command.Parameters.AddWithValue("@GroceryListId", item.GroceryListId);
                command.Parameters.AddWithValue("@ProductId", item.ProductId);
                command.Parameters.AddWithValue("@Amount", item.Amount);

                long newId = (long)command.ExecuteScalar();
                item.Id = (int)newId;
            }

            CloseConnection();
            return item;
        }

        public GroceryListItem? Delete(GroceryListItem item)
        {
            OpenConnection();
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"DELETE FROM GroceryListItem WHERE Id = @id";
                command.Parameters.AddWithValue(@"id", item.Id);
                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    item = null;
                }

                CloseConnection();
                return rowsAffected > 0 ? item : null;
            }
        }

        public GroceryListItem? Get(int id)
        {
            GroceryListItem? result = null;
            OpenConnection();

            using (var command = Connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT Id, GroceryListId, ProductId, Quantity FROM GroceryListItems WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = new GroceryListItem(
                            reader.GetInt32(0),
                            reader.GetInt32(1),
                            reader.GetInt32(2),
                            reader.GetInt32(3)
                        );
                    }
                }
            }

            CloseConnection();
            return result;
        }


        public GroceryListItem? Update(GroceryListItem item)
        {
            OpenConnection();
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE GroceryListItems
                    SET GroceryListId = @GroceryListId,
                        ProductId = @ProductId,
                        Quantity = @Quantity
                    WHERE Id = @Id";

                command.Parameters.AddWithValue("@GroceryListId", item.GroceryListId);
                command.Parameters.AddWithValue("@ProductId", item.ProductId);
                command.Parameters.AddWithValue("@Id", item.Id);
                command.Parameters.AddWithValue("@Quantity", item.Amount);

                int rows = command.ExecuteNonQuery();
                CloseConnection();

                return rows > 0 ? Get(item.Id) : null;

            }
        }
    }
}
