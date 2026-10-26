using Microsoft.Maui.Handlers;

namespace GermanyGlossary.Views
{
    // Removes the native underline / border / focus background from Entry and Editor,
    // so they can sit inside our own rounded "Field" Border and look the same on every platform.
    public static class BorderlessInputs
    {
        public static void Configure()
        {
            EntryHandler.Mapper.AppendToMapping("Borderless", (handler, _) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif IOS || MACCATALYST
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#elif WINDOWS
                MakeTransparent(handler.PlatformView);
#endif
            });

            EditorHandler.Mapper.AppendToMapping("Borderless", (handler, _) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif WINDOWS
                MakeTransparent(handler.PlatformView);
#endif
            });
        }

#if WINDOWS
        private static void MakeTransparent(Microsoft.UI.Xaml.Controls.TextBox textBox)
        {
            var transparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);

            // WinUI swaps these brushes on hover/focus, so overriding only Background is not enough.
            foreach (var key in new[]
            {
                "TextControlBackground", "TextControlBackgroundPointerOver", "TextControlBackgroundFocused",
                "TextControlBorderBrush", "TextControlBorderBrushPointerOver", "TextControlBorderBrushFocused"
            })
            {
                textBox.Resources[key] = transparent;
            }

            textBox.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            textBox.Background = transparent;
        }
#endif
    }
}
