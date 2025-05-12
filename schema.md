Group
- id
- title
    - type: string
    - required: true
- plan_hours
    - type: int
    - required: false

Task
- id
- title
    - type: string
    - required: true
- time_start
    - type: time
    - required: false
- date_start
    - type: date
    - required: true
- remind_time
    - type: time
    - required: false
- time_interval
    - type: int
    - required: true
- schedule
    - type: list
    - required: true
- is_regular
    - type: bool
    - required: true
- count_plan_intervals
    - type: int
    - required: false
- group_id:
    - type: link to Group (many to one)
    - required: true
- parent_id:
    - type: link to Task (many to one)
    - required: false

TaskEvent
- id
- task_id
    - type: link to Task (many to one)
    - required: true
- datetime_start
    - type: datetime
    - required: false
- datetime_end
    - type: datetime
    - required: false


Работа - 2 часа из 6 запланированных
-- Плановое значение: 6
    - Код
        -- Расписание: ПН, ВТ, СР, ЧТ, ПТ
        -- Таймер: 25 минут
        - Ревью
            -- Расписание: ПН, ВТ, СР, ЧТ, ПТ
            -- Таймер: 25 минут
    - Звонки
        -- Расписание: ПН, ВТ, СР, ЧТ, ПТ
        -- Таймер: 120 минут
        - Дейли
            -- Время: 12:00
            -- Напоминание: 11:55
            -- Расписание: ПН, ВТ, СР, ЧТ, ПТ
            -- Таймер: 120 минут
        - Созвон по такой-то теме
            -- Дата: 15.04.2025
            -- Время: 15:00
            -- Таймер: 120 минут

Саморазвитие
    - Чтение книг (0 из 1 помидорки)
        -- План помидорок: 1
        -- Расписание: ПН, ВТ, СР, ЧТ, ПТ, ВС
        -- Таймер: 25 минут
    - Обучение (1 из 2 помидорок)
        -- План помидорок: 2
        -- Расписание: ПН, ВТ, СР, ЧТ, ПТ
        -- Таймер: 25 минут