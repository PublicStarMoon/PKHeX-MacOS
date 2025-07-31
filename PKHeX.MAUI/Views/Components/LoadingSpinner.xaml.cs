using Microsoft.Maui.Controls;

namespace PKHeX.MAUI.Views.Components;

/// <summary>
/// Modern loading spinner with rotating animation for data initialization
/// Shows a stylish 刷新加载转圈 animation with Chinese text
/// </summary>
public partial class LoadingSpinner : ContentView
{
    private readonly Animation _rotationAnimation;
    private bool _isAnimating = false;

    public static readonly BindableProperty IsVisibleProperty =
        BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(LoadingSpinner), 
            false, propertyChanged: OnIsVisibleChanged);

    public static readonly BindableProperty LoadingTextProperty =
        BindableProperty.Create(nameof(LoadingText), typeof(string), typeof(LoadingSpinner), 
            "刷新加载中...", propertyChanged: OnLoadingTextChanged);

    public new bool IsVisible
    {
        get => (bool)GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    public string LoadingText
    {
        get => (string)GetValue(LoadingTextProperty);
        set => SetValue(LoadingTextProperty, value);
    }

    public LoadingSpinner()
    {
        InitializeComponent();
        
        // Create smooth rotation animation
        _rotationAnimation = new Animation(
            v => SpinnerPath.Rotation = v,
            0, 360,
            Easing.Linear);
    }

    private static void OnIsVisibleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LoadingSpinner spinner)
        {
            if ((bool)newValue)
            {
                spinner.StartAnimation();
            }
            else
            {
                spinner.StopAnimation();
            }
        }
    }

    private static void OnLoadingTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LoadingSpinner spinner)
        {
            spinner.LoadingLabel.Text = (string)newValue;
        }
    }

    private void StartAnimation()
    {
        if (_isAnimating) return;
        
        _isAnimating = true;
        this.Animate("SpinnerRotation", _rotationAnimation, 16, 1000, Easing.Linear, 
            finished: (v, c) => { }, repeat: () => _isAnimating);
    }

    private void StopAnimation()
    {
        if (!_isAnimating) return;
        
        _isAnimating = false;
        this.AbortAnimation("SpinnerRotation");
        SpinnerPath.Rotation = 0;
    }

    /// <summary>
    /// Show the loading spinner with custom text
    /// </summary>
    public void Show(string text = "刷新加载中...")
    {
        LoadingText = text;
        IsVisible = true;
    }

    /// <summary>
    /// Hide the loading spinner
    /// </summary>
    public void Hide()
    {
        IsVisible = false;
    }
}
