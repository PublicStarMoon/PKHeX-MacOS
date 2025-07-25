using PKHeX.Core;

namespace PKHeX.MAUI.Views;

public class MoveData
{
    public byte Type { get; set; }
    public byte PP { get; set; }
    public byte Power { get; set; }
    public byte Accuracy { get; set; }
}

public partial class MoveEditorPage : ContentPage
{
    private PKM? _pokemon;
    private bool _isUpdating;
    private readonly MoveChoiceViewModel[] _moveChoices;

    public MoveEditorPage()
    {
        InitializeComponent();
        _moveChoices = new MoveChoiceViewModel[4];
        
        for (int i = 0; i < 4; i++)
        {
            _moveChoices[i] = new MoveChoiceViewModel();
        }

        BindingContext = this;
    }

    public MoveEditorPage(PKM pokemon) : this()
    {
        _pokemon = pokemon;
        LoadMoveData();
    }

    public MoveChoiceViewModel[] MoveChoices => _moveChoices;

    private void LoadMoveData()
    {
        if (_pokemon == null) return;

        _isUpdating = true;

        try
        {
            var moves = _pokemon.Moves;
            var pps = _pokemon.PP_Ups;
            var currentPPs = _pokemon.Move_PP;

            for (int i = 0; i < 4; i++)
            {
                var move = moves[i];
                var moveData = move > 0 ? GetMoveData(move, _pokemon.Context) : null;
                
                _moveChoices[i].UpdateMove(move, moveData, pps[i], currentPPs[i], _pokemon);
            }

            UpdateRelearn();
            UpdateLegalMoves();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void UpdateRelearn()
    {
        if (_pokemon == null) return;

        var relearn = _pokemon.RelearnMoves;
        for (int i = 0; i < 4; i++)
        {
            var move = relearn[i];
            var moveData = move > 0 ? GetMoveData(move, _pokemon.Context) : null;
            
            switch (i)
            {
                case 0:
                    Relearn1Label.Text = GetMoveDisplayText(move, moveData);
                    break;
                case 1:
                    Relearn2Label.Text = GetMoveDisplayText(move, moveData);
                    break;
                case 2:
                    Relearn3Label.Text = GetMoveDisplayText(move, moveData);
                    break;
                case 3:
                    Relearn4Label.Text = GetMoveDisplayText(move, moveData);
                    break;
            }
        }
    }

    private void UpdateLegalMoves()
    {
        if (_pokemon == null) return;

        try
        {
            var encounters = EncounterMovesetGenerator.GenerateEncounters(_pokemon);
            var movesets = encounters.SelectMany(enc => enc.GetAllMoves()).Distinct().ToArray();
            var legalMoves = movesets.Where(m => m > 0).OrderBy(m => m).ToList();

            LegalMovesLabel.Text = $"Legal moves available: {legalMoves.Count}";
        }
        catch
        {
            LegalMovesLabel.Text = "Legal moves: Unable to calculate";
        }
    }

    private string GetMoveDisplayText(ushort move, MoveData? moveData)
    {
        if (move == 0) return "None";
        
        var moveName = GameInfo.Strings.Move[move];
        if (moveData != null)
        {
            var type = GameInfo.Strings.types[moveData.Type];
            return $"{moveName} ({type})";
        }
        
        return moveName;
    }

    private async void OnMoveSelected(object sender, EventArgs e)
    {
        if (_pokemon == null || _isUpdating) return;

        var button = (Button)sender;
        var moveIndex = int.Parse(button.CommandParameter.ToString()!);

        var availableMoves = GetAvailableMoves();
        var selectedMove = await DisplayActionSheet("Select Move", "Cancel", null, availableMoves);

        if (selectedMove == "Cancel" || selectedMove == null) return;

        var moveId = GetMoveIdFromName(selectedMove);
        if (moveId == 0) return;

        var moves = _pokemon.Moves;
        moves[moveIndex] = moveId;
        _pokemon.Moves = moves;

        // Reset PP to max
        var pps = _pokemon.Move_PP;
        var moveData = GetMoveData(moveId, _pokemon.Context);
        pps[moveIndex] = moveData?.PP ?? 0;
        _pokemon.Move_PP = pps;

        LoadMoveData();
    }

    private string[] GetAvailableMoves()
    {
        var moves = new List<string> { "None" };
        
        for (int i = 1; i < GameInfo.Strings.Move.Count; i++)
        {
            var moveName = GameInfo.Strings.Move[i];
            if (!string.IsNullOrEmpty(moveName))
            {
                var moveData = GetMoveData(i, _pokemon?.Context ?? EntityContext.Gen9);
                var type = moveData != null ? GameInfo.Strings.types[moveData.Type] : "Unknown";
                moves.Add($"{moveName} ({type})");
            }
        }

        return moves.ToArray();
    }

    private ushort GetMoveIdFromName(string moveName)
    {
        if (moveName == "None") return 0;

        var cleanName = moveName.Split('(')[0].Trim();
        
        for (int i = 1; i < GameInfo.Strings.Move.Count; i++)
        {
            if (GameInfo.Strings.Move[i] == cleanName)
                return (ushort)i;
        }

        return 0;
    }

    private async void OnPPUpChanged(object sender, EventArgs e)
    {
        if (_pokemon == null || _isUpdating) return;

        var stepper = (Stepper)sender;
        var moveIndex = int.Parse(stepper.ClassId);
        var newValue = (int)stepper.Value;

        var ppUps = _pokemon.PP_Ups;
        ppUps[moveIndex] = newValue;
        _pokemon.PP_Ups = ppUps;

        // Update current PP to max
        var moves = _pokemon.Moves;
        var moveData = GetMoveData(moves[moveIndex], _pokemon.Context);
        if (moveData != null)
        {
            var maxPP = moveData.PP + (moveData.PP / 5 * newValue);
            var currentPP = _pokemon.Move_PP;
            currentPP[moveIndex] = maxPP;
            _pokemon.Move_PP = currentPP;
        }

        LoadMoveData();
    }

    private async void OnCurrentPPChanged(object sender, TextChangedEventArgs e)
    {
        if (_pokemon == null || _isUpdating) return;

        var entry = (Entry)sender;
        var moveIndex = int.Parse(entry.ClassId);

        if (int.TryParse(e.NewTextValue, out var newPP))
        {
            var currentPP = _pokemon.Move_PP;
            currentPP[moveIndex] = Math.Max(0, newPP);
            _pokemon.Move_PP = currentPP;
        }
    }

    private async void OnSuggestMovesClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;

        try
        {
            var suggestion = MoveListSuggest.GetSuggestedCurrentMoves(_pokemon, null);
            if (suggestion != null && suggestion.Length >= 4)
            {
                _pokemon.Moves = suggestion;
                
                // Set PP to max for all moves
                var pps = new int[4];
                for (int i = 0; i < 4; i++)
                {
                    var moveData = GetMoveData(suggestion[i], _pokemon.Context);
                    pps[i] = moveData?.PP ?? 0;
                }
                _pokemon.Move_PP = pps;

                LoadMoveData();
                await DisplayAlert("Success", "Moves suggested based on level and legality!", "OK");
            }
            else
            {
                await DisplayAlert("No Suggestions", "Unable to suggest moves for this Pokémon.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to suggest moves: {ex.Message}", "OK");
        }
    }

    private async void OnMaxPPClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;

        var moves = _pokemon.Moves;
        var ppUps = _pokemon.PP_Ups;
        var currentPP = new int[4];

        for (int i = 0; i < 4; i++)
        {
            var moveData = GetMoveData(moves[i], _pokemon.Context);
            if (moveData != null)
            {
                currentPP[i] = moveData.PP + (moveData.PP / 5 * ppUps[i]);
            }
        }

        _pokemon.Move_PP = currentPP;
        LoadMoveData();
        await DisplayAlert("Success", "All PP restored to maximum!", "OK");
    }

    private static MoveData? GetMoveData(ushort moveId, EntityContext context)
    {
        if (moveId == 0) return null;

        return new MoveData
        {
            Type = 1, // Normal type by default - in a real implementation, you'd need move type data
            PP = MoveInfo.GetPP(context, moveId),
            Power = 60, // Default power - in a real implementation, you'd need move power data  
            Accuracy = 100 // Default accuracy - in a real implementation, you'd need move accuracy data
        };
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

public class MoveChoiceViewModel
{
    public string MoveName { get; set; } = "None";
    public string MoveType { get; set; } = "";
    public string MovePower { get; set; } = "";
    public string MoveAccuracy { get; set; } = "";
    public int PPUps { get; set; }
    public int CurrentPP { get; set; }
    public int MaxPP { get; set; }
    public string PPDisplay => $"{CurrentPP}/{MaxPP}";
    public Color TypeColor { get; set; } = Colors.Gray;

    public void UpdateMove(ushort moveId, MoveData? moveData, int ppUps, int currentPP, PKM pokemon)
    {
        if (moveId == 0 || moveData == null)
        {
            MoveName = "None";
            MoveType = "";
            MovePower = "";
            MoveAccuracy = "";
            PPUps = 0;
            CurrentPP = 0;
            MaxPP = 0;
            TypeColor = Colors.Gray;
            return;
        }

        MoveName = GameInfo.Strings.Move[moveId];
        MoveType = GameInfo.Strings.types[moveData.Type];
        MovePower = moveData.Power == 0 ? "—" : moveData.Power.ToString();
        MoveAccuracy = moveData.Accuracy == 0 ? "—" : moveData.Accuracy.ToString();
        PPUps = ppUps;
        CurrentPP = currentPP;
        MaxPP = moveData.PP + (moveData.PP / 5 * ppUps);
        TypeColor = GetTypeColor(moveData.Type);
    }

    private static Color GetTypeColor(byte type)
    {
        return type switch
        {
            1 => Color.FromRgb(168, 167, 122), // Normal
            2 => Color.FromRgb(194, 46, 40),   // Fighting
            3 => Color.FromRgb(169, 143, 243), // Flying
            4 => Color.FromRgb(163, 62, 161),  // Poison
            5 => Color.FromRgb(226, 191, 101), // Ground
            6 => Color.FromRgb(182, 161, 54),  // Rock
            7 => Color.FromRgb(166, 185, 26),  // Bug
            8 => Color.FromRgb(115, 87, 151),  // Ghost
            9 => Color.FromRgb(183, 183, 206), // Steel
            10 => Color.FromRgb(238, 129, 48), // Fire
            11 => Color.FromRgb(99, 144, 240), // Water
            12 => Color.FromRgb(122, 199, 76), // Grass
            13 => Color.FromRgb(249, 207, 48), // Electric
            14 => Color.FromRgb(249, 85, 135), // Psychic
            15 => Color.FromRgb(150, 217, 214), // Ice
            16 => Color.FromRgb(111, 53, 252), // Dragon
            17 => Color.FromRgb(112, 87, 70),  // Dark
            18 => Color.FromRgb(214, 133, 173), // Fairy
            _ => Colors.Gray
        };
    }
}
