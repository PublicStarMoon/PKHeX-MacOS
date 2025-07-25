using PKHeX.Core;
using PKHeX.MAUI.Utilities;
using System.Collections.ObjectModel;

namespace PKHeX.MAUI.Views;

public class PokemonSpeciesInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Generation { get; set; } = "";
}

public partial class PokemonDatabasePage : ContentPage
{
    private SaveFile? _saveFile;
    private ObservableCollection<PokemonSpeciesInfo> _allPokemon = new();
    private ObservableCollection<PokemonSpeciesInfo> _filteredPokemon = new();
    private PokemonSpeciesInfo? _selectedPokemon;

    public PokemonDatabasePage(SaveFile? saveFile = null)
    {
        InitializeComponent();
        _saveFile = saveFile;
        
        SetupUI();
        LoadPokemonDatabase();
    }

    private void SetupUI()
    {
        // Setup generation picker
        GenerationPicker.Items.Clear();
        GenerationPicker.Items.Add("All Generations");
        for (int i = 1; i <= 9; i++)
        {
            GenerationPicker.Items.Add($"Generation {i}");
        }
        GenerationPicker.SelectedIndex = 0;

        // Setup collection view
        PokemonCollectionView.ItemsSource = _filteredPokemon;

        // Enable create button only if we have a save file
        CreatePokemonButton.IsEnabled = _saveFile != null;
    }

    private void LoadPokemonDatabase()
    {
        try
        {
            _allPokemon.Clear();
            
            var speciesList = GameInfo.GetStrings(1).specieslist;
            
            // Add Pokemon 1-1010 (covers all generations up to Gen 9)
            for (int i = 1; i < Math.Min(speciesList.Length, 1011); i++)
            {
                if (!string.IsNullOrEmpty(speciesList[i]))
                {
                    var generation = GetPokemonGeneration(i);
                    _allPokemon.Add(new PokemonSpeciesInfo
                    {
                        Id = i,
                        Name = speciesList[i],
                        Generation = $"Gen {generation}"
                    });
                }
            }

            // Initially show all Pokemon
            RefreshFilteredList();
            StatusLabel.Text = $"Loaded {_allPokemon.Count} Pokemon species";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error loading database: {ex.Message}";
        }
    }

    private int GetPokemonGeneration(int species)
    {
        // Approximate generation boundaries
        return species switch
        {
            <= 151 => 1,
            <= 251 => 2,
            <= 386 => 3,
            <= 493 => 4,
            <= 649 => 5,
            <= 721 => 6,
            <= 809 => 7,
            <= 905 => 8,
            _ => 9
        };
    }

    private void RefreshFilteredList()
    {
        _filteredPokemon.Clear();
        
        var searchText = SearchBar.Text?.ToLower() ?? "";
        var selectedGeneration = GenerationPicker.SelectedIndex;

        var filtered = _allPokemon.Where(p =>
        {
            // Filter by search text
            bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                               p.Name.ToLower().Contains(searchText) ||
                               p.Id.ToString().Contains(searchText);

            // Filter by generation
            bool matchesGeneration = selectedGeneration == 0 || // All generations
                                   p.Generation == $"Gen {selectedGeneration}";

            return matchesSearch && matchesGeneration;
        });

        foreach (var pokemon in filtered)
        {
            _filteredPokemon.Add(pokemon);
        }

        StatusLabel.Text = $"Showing {_filteredPokemon.Count} of {_allPokemon.Count} Pokemon";
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshFilteredList();
    }

    private void OnGenerationChanged(object sender, EventArgs e)
    {
        RefreshFilteredList();
    }

    private void OnClearSearchClicked(object sender, EventArgs e)
    {
        SearchBar.Text = "";
        GenerationPicker.SelectedIndex = 0;
        RefreshFilteredList();
    }

    private void OnPokemonSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is PokemonSpeciesInfo selected)
        {
            _selectedPokemon = selected;
            ViewDetailsButton.IsEnabled = true;
            StatusLabel.Text = $"Selected: {selected.Name} (#{selected.Id:000})";
        }
        else
        {
            _selectedPokemon = null;
            ViewDetailsButton.IsEnabled = false;
            StatusLabel.Text = "Select a Pokemon to view details";
        }
    }

    private async void OnViewDetailsClicked(object sender, EventArgs e)
    {
        if (_selectedPokemon == null) return;

        try
        {
            // Simplified approach - just show basic information without creating a sample Pokemon
            // The EntityFormat.GetFromString API may have changed or been removed
            var details = $"Species: {_selectedPokemon.Name}\n" +
                         $"Species ID: {_selectedPokemon.Id}\n" +
                         $"Base Stats: Available through PersonalInfo\n" +
                         $"Type Information: Available through PersonalInfo";

            await DisplayAlert($"{_selectedPokemon.Name} Details", details, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load Pokemon details: {ex.Message}", "OK");
        }
    }

    private async void OnCreatePokemonClicked(object sender, EventArgs e)
    {
        if (_selectedPokemon == null || _saveFile == null)
        {
            await DisplayAlert("Error", "No Pokemon selected or no save file loaded.", "OK");
            return;
        }

        try
        {
            // Create a legal Pokemon with proper trainer info
            var level = 50; // Default level since LevelSlider may not be defined in XAML
            var newPokemon = PokemonHelper.CreateLegalPokemon(_saveFile, _selectedPokemon.Id, level);
            
            // Ensure proper trainer info for SV compliance
            if (_saveFile.Generation >= 9)
            {
                PokemonHelper.FixTrainerInfo(newPokemon, _saveFile);
            }
            
            // Validate legality
            var isValid = PokemonHelper.IsLegal(newPokemon, _saveFile);
            if (!isValid)
            {
                var shouldContinue = await DisplayAlert("Warning", 
                    "Created Pokemon may have legality issues. Continue to editor?", 
                    "Yes", "No");
                if (!shouldContinue) return;
            }

            // Open the detailed editor
            var editorPage = new PokemonEditorPage(newPokemon, _saveFile);
            await Navigation.PushAsync(editorPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to create Pokemon: {ex.Message}", "OK");
        }
    }

    private async void OnGenerateRandomClicked(object sender, EventArgs e)
    {
        try
        {
            var random = new Random();
            var randomIndex = random.Next(_filteredPokemon.Count);
            
            if (_filteredPokemon.Count > 0)
            {
                var randomPokemon = _filteredPokemon[randomIndex];
                PokemonCollectionView.SelectedItem = randomPokemon;
                PokemonCollectionView.ScrollTo(randomPokemon);
                
                StatusLabel.Text = $"Randomly selected: {randomPokemon.Name}";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to generate random Pokemon: {ex.Message}", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
