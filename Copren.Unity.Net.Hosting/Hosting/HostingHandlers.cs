using System.Threading.Tasks;
using Copren.Unity.Net.Domain;

namespace Copren.Unity.Net.Hosting.Hosting
{
    public delegate Task ConnectionHandler(Client client);
}