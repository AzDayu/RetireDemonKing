using System;
using UnityEngine.UIElements;

public class MonsterViewModel : ViewModelBase
{
	private readonly MonsterModel _monsterModel;

	public MonsterViewModel(MonsterModel monsterModel)
	{
		_monsterModel = monsterModel;
		_monsterModel.OnInfoChanged += HandleModelInfoChanged;
	}

	public void InvokeOnceOnInit()
	{
		OnPropertyChanged(nameof(HpRatio));
	}

	private void HandleModelInfoChanged(string propertyName)
	{
		if (propertyName == nameof(MonsterModel.CurHp) || propertyName == nameof(MonsterModel.MaxHp))
		{
			OnPropertyChanged(nameof(HpRatio));
		}
	}

	public void Dispose()
	{
		_monsterModel.OnInfoChanged -= HandleModelInfoChanged;
	}

	public float HpRatio => _monsterModel.MaxHp > 0f ? _monsterModel.CurHp / _monsterModel.MaxHp : 0f;
}