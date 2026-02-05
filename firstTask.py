import os
import json
import time
from pathlib import Path

def simple_steam_monitor():
    """Упрощенный мониторинг загрузок Steam"""
    
    steam_paths = [
        Path("C:/Program Files/Steam"),
        Path("C:/Program Files (x86)/Steam"),
    ]
    
    steam_path = None
    for path in steam_paths:
        if path.exists():
            steam_path = path
            break
    
    if not steam_path:
        print("Steam не найден!")
        return
    
    print("Мониторинг загрузок Steam (5 минут)")
    print("=" * 40)
    
    for i in range(5):
        stats_file = steam_path / "appcache" / "stats" / "DownloadingStats.json"
        
        if stats_file.exists():
            try:
                with open(stats_file, 'r') as f:
                    data = json.load(f)
                
                if data:
                    speed = data.get('DownloadRate', 0) / (1024*1024)
                    state = "Загрузка" if data.get('State') == 1 else "Пауза"
                    app_id = data.get('AppID', 'Неизвестно')
                    
                    print(f"\nПроверка {i+1}/5:")
                    print(f"  Игра: AppID {app_id}")
                    print(f"  Скорость: {speed:.2f} МБ/сек")
                    print(f"  Статус: {state}")
                else:
                    print(f"\nПроверка {i+1}/5: Нет активных загрузок")
                    
            except Exception as e:
                print(f"\nПроверка {i+1}/5: Ошибка: {e}")
        else:
            print(f"\nПроверка {i+1}/5: Файл статистики не найден")
        
        time.sleep(60)
    
    print("\n" + "=" * 40)
    print("Мониторинг завершен")

if __name__ == "__main__":
    simple_steam_monitor()
