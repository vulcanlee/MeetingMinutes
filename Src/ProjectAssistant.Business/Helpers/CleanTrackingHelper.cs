using Microsoft.EntityFrameworkCore;
using ProjectAssistant.EntityModel;

namespace ProjectAssistant.Business.Helpers;

public class CleanTrackingHelper
{
    public static void Clean<T>(BackendDBContext context) where T : class
    {
        foreach (var fooXItem in context.Set<T>().Local)
        {
            context.Entry(fooXItem).State = EntityState.Detached;
        }
    }
}
