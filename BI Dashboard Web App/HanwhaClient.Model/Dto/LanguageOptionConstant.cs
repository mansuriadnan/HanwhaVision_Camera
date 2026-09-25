using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public static class LanguageOptionConstant
    {
        public static readonly List<LanguageOption> LANGUAGE_OPTIONS = new List<LanguageOption>
                                  {
                                      new LanguageOption { Id = "en", Title = "English" },
                                      new LanguageOption { Id = "ar", Title = "Arabic" },
                                      new LanguageOption { Id = "zh", Title = "Chinese" },
                                      new LanguageOption { Id = "cs", Title = "Czech" },
                                      new LanguageOption { Id = "da", Title = "Danish" },
                                      new LanguageOption { Id = "fr", Title = "French" },
                                      new LanguageOption { Id = "de", Title = "German" },
                                      new LanguageOption { Id = "it", Title = "Italian" },
                                      new LanguageOption { Id = "ko", Title = "Korean" },
                                      new LanguageOption { Id = "es", Title = "Spanish" },
                                      new LanguageOption { Id = "ms", Title = "Malay" },
                                      new LanguageOption { Id = "sv", Title = "Swedish" },
                                      new LanguageOption { Id = "pl", Title = "Polish" },
                                      new LanguageOption { Id = "pt", Title = "Portuguese" },
                                      new LanguageOption { Id = "no", Title = "Norwegian" },
                                      new LanguageOption { Id = "hu", Title = "Hungarian" }
                                  };
    }
}
