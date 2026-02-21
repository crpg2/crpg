using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace Crpg.Module.GUI.TrainingGround;
internal class CrpgTrainingGroundTargetMarkerListPanel : ListPanel
{
    private const string DefaultState = "Default";
    private const string FocusedState = "Focused";
    private const string TrackedState = "Tracked";
    private Vec2 _position;
    private bool _isAgentInScreenBoundaries;
    private bool _isAvailable;
    private bool _isTracked;
    private bool _isAgentFocused;
    private bool _hasTargetSentDuelRequest;
    private bool _hasPlayerSentDuelRequest;
    private int _wSign;
    private int _rating;
    private RichTextWidget _actionText = null!;
    private BrushWidget _background = null!;
    private BrushWidget _border = null!;
    private TextWidget _ratingTextWidget = null!;
    [Editor]
    public Vec2 Position
    {
        get
        {
            return _position;
        }
        set
        {
            if (value != _position)
            {
                _position = value;
                OnPropertyChanged(value);
            }
        }
    }

    [Editor]
    public bool IsAgentInScreenBoundaries
    {
        get
        {
            return _isAgentInScreenBoundaries;
        }
        set
        {
            if (value != _isAgentInScreenBoundaries)
            {
                _isAgentInScreenBoundaries = value;
                OnPropertyChanged(value);
            }
        }
    }

    [Editor]
    public bool IsAvailable
    {
        get
        {
            return _isAvailable;
        }
        set
        {
            if (value != _isAvailable)
            {
                _isAvailable = value;
                OnPropertyChanged(value);
            }
        }
    }

    [Editor]
    public bool IsTracked
    {
        get
        {
            return _isTracked;
        }
        set
        {
            if (value != _isTracked)
            {
                _isTracked = value;
                OnPropertyChanged(value);
            }
        }
    }

    [Editor]
    public bool IsAgentFocused
    {
        get
        {
            return _isAgentFocused;
        }
        set
        {
            if (value != _isAgentFocused)
            {
                _isAgentFocused = value;
                OnPropertyChanged(value);
                UpdateChildrenFocusStates();
            }
        }
    }

    [Editor]
    public bool HasTargetSentDuelRequest
    {
        get
        {
            return _hasTargetSentDuelRequest;
        }
        set
        {
            if (value != _hasTargetSentDuelRequest)
            {
                _hasTargetSentDuelRequest = value;
                OnPropertyChanged(value);
                UpdateChildrenFocusStates();
            }
        }
    }

    [Editor]
    public bool HasPlayerSentDuelRequest
    {
        get
        {
            return _hasPlayerSentDuelRequest;
        }
        set
        {
            if (value != _hasPlayerSentDuelRequest)
            {
                _hasPlayerSentDuelRequest = value;
                OnPropertyChanged(value);
                UpdateChildrenFocusStates();
            }
        }
    }

    [Editor]
    public int WSign
    {
        get
        {
            return _wSign;
        }
        set
        {
            if (_wSign != value)
            {
                _wSign = value;
                OnPropertyChanged(value);
            }
        }
    }

    [Editor]
    public int Rating
    {
        get
        {
            return _rating;
        }
        set
        {
            if (_rating != value)
            {
                _rating = value;
                OnPropertyChanged(value);
                UpdateRatingState();
            }
        }
    }

    [Editor]
    public RichTextWidget ActionText
    {
        get
        {
            return _actionText;
        }
        set
        {
            if (value != _actionText)
            {
                _actionText = value;
                OnPropertyChanged(value);
            }
        }
    }

    [Editor]
    public BrushWidget Background
    {
        get
        {
            return _background;
        }
        set
        {
            if (value != _background)
            {
                _background = value;
                OnPropertyChanged(value);
            }
        }
    }

    [Editor]
    public BrushWidget Border
    {
        get
        {
            return _border;
        }
        set
        {
            if (value != _border)
            {
                _border = value;
                OnPropertyChanged(value);
            }
        }
    }

    [Editor]
    public TextWidget RatingTextWidget
    {
        get
        {
            return _ratingTextWidget;
        }
        set
        {
            if (value != _ratingTextWidget)
            {
                _ratingTextWidget = value;
                OnPropertyChanged(value);
            }
        }
    }

    public CrpgTrainingGroundTargetMarkerListPanel(UIContext context)
        : base(context)
    {
    }

    protected override void OnLateUpdate(float dt)
    {
        if (!IsAvailable)
        {
            IsVisible = false;
            return;
        }

        float x = Context.EventManager.PageSize.X;
        float y = Context.EventManager.PageSize.Y;
        Vec2 position = Position;
        if (WSign > 0 && position.x - Size.X / 2f > 0f && position.x + Size.X / 2f < Context.EventManager.PageSize.X && position.y > 0f && position.y + Size.Y < Context.EventManager.PageSize.Y)
        {
            ScaledPositionXOffset = position.x - Size.X / 2f;
            ScaledPositionYOffset = position.y - Size.Y - 20f;
            _actionText.ScaledPositionXOffset = ScaledPositionXOffset;
            _actionText.ScaledPositionYOffset = ScaledPositionYOffset + Size.Y;
            IsVisible = true;
        }
        else if (IsTracked)
        {
            Vec2 vec = new Vec2(Context.EventManager.PageSize.X / 2f, Context.EventManager.PageSize.Y / 2f);
            position -= vec;
            if (WSign < 0)
            {
                position *= -1f;
            }

            float radian = Mathf.Atan2(position.y, position.x) - (float)Math.PI / 2f;
            float num = Mathf.Cos(radian);
            float num2 = Mathf.Sin(radian);
            position = vec + new Vec2(num2 * 150f, num * 150f);
            float num3 = num / num2;
            Vec2 vec2 = vec * 1f;
            position = num > 0f ? new Vec2((0f - vec2.y) / num3, vec.y) : new Vec2(vec2.y / num3, 0f - vec.y);
            if (position.x > vec2.x)
            {
                position = new Vec2(vec2.x, (0f - vec2.x) * num3);
            }
            else if (position.x < 0f - vec2.x)
            {
                position = new Vec2(0f - vec2.x, vec2.x * num3);
            }

            position += vec;
            ScaledPositionXOffset = Mathf.Clamp(position.x - Size.X / 2f, 0f, x - Size.X);
            ScaledPositionYOffset = Mathf.Clamp(position.y - Size.Y, 0f, y - Size.Y);
            IsVisible = true;
        }
        else
        {
            IsVisible = false;
        }
    }

    private void UpdateChildrenFocusStates()
    {
        string state = HasTargetSentDuelRequest ? TrackedState : HasPlayerSentDuelRequest || IsAgentFocused ? FocusedState : DefaultState;
        Background.SetState(state);
        Border.SetState(state);
    }

    private void UpdateRatingState()
    {
        string state;
        if (Rating > 1750)
        {
            state = "Champion";
        }
        else if (Rating > 1500)
        {
            state = "Diamond";
        }
        else if (Rating > 1250)
        {
            state = "Platinum";
        }
        else if (Rating > 1000)
        {
            state = "Gold";
        }
        else if (Rating > 750)
        {
            state = "Silver";
        }
        else if (Rating > 500)
        {
            state = "Bronze";
        }
        else if (Rating > 250)
        {
            state = "Copper";
        }
        else
        {
            state = "Iron";
        }

        RatingTextWidget.SetState(state);
    }
}
