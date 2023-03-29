using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sandwych.Jasper;

public interface IMemberAccessor {
    object Get(object obj, string name, ParsingContext ctx);
}

public interface IAsyncMemberAccessor : IMemberAccessor {
    Task<object> GetAsync(object obj, string name, ParsingContext ctx);
}

