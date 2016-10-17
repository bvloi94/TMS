using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace TMS.ViewModels
{
    public class ModelErrorViewModel : List<ModelError>
    {
        public ModelErrorViewModel()
        {

        }

        public ModelErrorViewModel(ModelStateDictionary modelState)
        {
            if (modelState.IsValid) return;
            
            foreach (var state in modelState)
            {
                if (modelState[state.Key].Errors.Any())
                {
                    foreach (var error in modelState[state.Key].Errors)
                    {
                        Add(new ModelError
                        {
                            Name = state.Key,
                            Message = error.ErrorMessage
                        });
                    }
                }
            }
        }
    }

    public class ModelError
    {
        public string Name { get; set; }
        public string Message { get; set; }
    }
}
