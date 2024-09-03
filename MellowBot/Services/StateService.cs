using System.Collections.Concurrent;

namespace MellowBot.Services
{
    public class StateService
    {
        private readonly Dictionary<long, UserState> _userStates = new();

        private readonly ConcurrentDictionary<(long ChatId, string Button), DateTime> _buttonTimers = new();

        public UserState GetUserState(long chatId)
        {
            if (!_userStates.TryGetValue(chatId, out var state))
            {
                state = new UserState();
                _userStates[chatId] = state;
            }

            return state;
        }

        public void IncrementTroopCount(long chatId, string troopType)
        {
            var state = GetUserState(chatId);
            switch (troopType)
            {
                case "Infantry":
                    state.InfantryCatCount++;
                    break;
                case "Cavalry":
                    state.CavalryCatCount++;
                    break;
            }
        }

        public (string Text, string CallbackData) GetButtonState(long chatId, string button)
        {
            var buttonKey = (chatId, button);

            if (_buttonTimers.TryGetValue(buttonKey, out var timerEnd) && DateTime.UtcNow < timerEnd)
            {
                var timeLeft = (timerEnd - DateTime.UtcNow).TotalSeconds;
                return ($"{button} (Ready in {timeLeft:0} seconds)", "disabled");
            }

            return ($"{button}", button);
        }

        public bool IsButtonDisabled((long ChatId, string Button) buttonKey)
        {
            return _buttonTimers.TryGetValue(buttonKey, out var timerEnd) && DateTime.UtcNow < timerEnd;
        }

        public DateTime GetButtonReactivationTime((long ChatId, string Button) buttonKey)
        {
            _buttonTimers.TryGetValue(buttonKey, out var timerEnd);
            return timerEnd;
        }

        public void SetButtonCooldown((long ChatId, string Button) buttonKey, TimeSpan cooldown)
        {
            var newTimerEnd = DateTime.UtcNow.Add(cooldown);
            _buttonTimers[buttonKey] = newTimerEnd;
        }
    }

    public class UserState
    {
        public int InfantryCatCount { get; set; }
        public int CavalryCatCount { get; set; }
    }
}