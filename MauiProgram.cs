using GermanyGlossary.Data;
using GermanyGlossary.Repositories;
using GermanyGlossary.Services;
using GermanyGlossary.ViewModels;
using GermanyGlossary.Views;
using Microsoft.Extensions.Logging;

namespace GermanyGlossary
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            BorderlessInputs.Configure();

            builder.Services.AddSingleton<AppDatabase>();
            builder.Services.AddSingleton<IWordRepository, WordRepository>();
            builder.Services.AddSingleton<IReviewStateRepository, ReviewStateRepository>();

            builder.Services.AddSingleton<IClock, SystemClock>();
            builder.Services.AddSingleton<ISpacedRepetitionAlgorithm, Sm2Algorithm>();
            builder.Services.AddSingleton<IReviewSessionService, ReviewSessionService>();

            builder.Services.AddTransient<WordsViewModel>();
            builder.Services.AddTransient<WordsPage>();
            builder.Services.AddTransient<WordEditViewModel>();
            builder.Services.AddTransient<WordEditPage>();
            builder.Services.AddTransient<FlashcardsViewModel>();
            builder.Services.AddTransient<FlashcardsPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
