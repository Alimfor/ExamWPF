using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Exda.WorkingWithDB
{
    public interface IUserInformation
    {
        void WriteInfoAboutUser(TextBox tbLastName, TextBox tbName);
        void WriteUserAnswer(Grid testingGrid);
    }
}
