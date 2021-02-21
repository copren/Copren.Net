using System.Threading.Tasks;
using Copren.Net.Domain;

namespace Copren.Net.Hosting.Hosting
{
    public delegate Task ConnectionHandler(Client client);
}