using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace SigmaLib.Interfaces
{
    public interface IDatabaseService
    {
        SqliteConnection GetConnection();
    }
}
