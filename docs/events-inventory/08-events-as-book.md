# Harvey и фермер: содержание событий

*Читабельная версия сцен мода HarveyOverhaul. Имена и реплики — из Content Patcher; технические команды (`warp`, `fork`, `Random`) опущены. Перед каждой сценой — **условия срабатывания** (CP preconditions, C# InjuryCare, SpaceCore triggers).*

---

## Оглавление

1. [Часть I. Первая встреча](#часть-i-первая-встреча)
   1.1 [Автобусная остановка](#eventharveyfirstmeeting-автобусная-остановка)
   1.2 [Первый осмотр](#eventharveycheckup-первый-осмотр)
   1.3 [Визит на ферму](#eventharveyfirstvisit-визит-на-ферму)
   1.4 [Второй визит — травяной чай](#eventharveysecondvisit-второй-визит-травяной-чай)
   1.5 [Прогулка в лес](#eventharveyfirstwalk-прогулка-в-лес)
2. [Часть II. История доверия (HarveyOverhaul Story)](#часть-ii-история-доверия-harveyoverhaul-story)
   2.1 [Скользкая дорожка](#harveyoverhaulstory-e1-slipperypath-скользкая-дорожка)
   2.2 [Настойчивый осмотр](#harveyoverhaulstory-e2-insistentexam-настойчивый-осмотр)
   2.3 [Лесная аптека](#harveyoverhaulstory-e3-forestapothecary-лесная-аптека)
   2.4 [Дыхание у пирса](#harveyoverhaulstory-e4-pierbreath-дыхание-у-пирса)
   2.5 [Рядом в грозу](#harveyoverhaulstory-e5-stormbeside-рядом-в-грозу)
   2.6 [Сказать вслух](#harveyoverhaulstory-e6-sayitoutloud-сказать-вслух)
   2.7 [Глоток солнца в городе](#harveyoverhaulstory-e7-townsip-sunny-глоток-солнца-в-городе)
   2.8 [Тихая полка](#harveyoverhaulstory-e8-quietshelf-тихая-полка)
3. [Часть III. Лечение и клиника](#часть-iii-лечение-и-клиника)
   3.1 [Первое серьёзное лечение](#harveymod-firsttreatment-первое-серьёзное-лечение)
   3.2 [Ночной кризис (dating/married)](#harveymod-nightcrisis-dating-ночной-кризис-dating-married)
   3.3 [Ночной кризис (до dating)](#harveymod-nightcrisis-predating-ночной-кризис-до-dating)
   3.4 [День рождения в больнице (dating)](#harveymod-birthdayhospital-dating-день-рождения-в-больнице-dating)
   3.5 [День рождения в больнице (друг)](#harveymod-birthdayhospital-friend-день-рождения-в-больнице-друг)
   3.6 [План лечения](#harveymod-treatmentplanmeeting-план-лечения)
   3.7 [Медосмотр по напоминанию (pre-dating)](#eventharveymedicalcheck-медосмотр-по-напоминанию-pre-dating)
   3.8 [Медосмотр по напоминанию (dating)](#eventharveymedicalcheck-dating-медосмотр-по-напоминанию-dating)
   3.9 [Осмотр старых шрамов](#eventharveytraumaexam-осмотр-старых-шрамов)
   3.10 [Экстренная помощь](#eventharveyemergencycare-экстренная-помощь)
   3.11 [Истощение](#eventharveyexhaustion-истощение)
   3.12 [Коллапс на ферме](#eventharveytreatmentcollapse-коллапс-на-ферме)
   3.13 [Остаёшься в палате](#eventstayinhospital-остаёшься-в-палате)
4. [Часть IV. Шахта и раны (InjuryCare)](#часть-iv-шахта-и-раны-injurycare)
   4.1 [Спасение из шахты (любовь)](#eventharveyminerescuedating-спасение-из-шахты-любовь)
   4.2 [Спасение из шахты](#eventharveyminerescue-спасение-из-шахты)
   4.3 [Лёгкое спасение из шахты](#eventharveyminorminerescue-лёгкое-спасение-из-шахты)
   4.4 [Перехват у входа в шахту](#eventharveymineinterception-перехват-у-входа-в-шахту)
   4.5 [Пещера черепов](#eventharveyskullcaveprevention-пещера-черепов)
5. [Часть V. Забота и ночные тревоги](#часть-v-забота-и-ночные-тревоги)
   5.1 [Проверка после обморока](#eventharveycheckhealthfarmer-проверка-после-обморока)
   5.2 [Ночная прогулка](#eventharveycheckfarmeroutsideafter22-ночная-прогулка)
   5.3 [Утренний осмотр](#eventharveymorningcheckup-утренний-осмотр)
   5.4 [Обморок в городе](#eventharveylatenightcollapse-обморок-в-городе)
6. [Часть VI. Гроза и страх](#часть-vi-гроза-и-страх)
   6.1 [Операция спасения](#eventrescueoperation-операция-спасения)
7. [Часть VII. Сердце и свидания](#часть-vii-сердце-и-свидания)
   7.1 [Первое свидание](#eventharveyfirstdate-первое-свидание)
   7.2 [Свидание в горах](#eventharveymountaindate-свидание-в-горах)
   7.3 [Предложение](#eventharveypropose-предложение)
   7.4 [Осмотр в комнате](#eventharveyroomcheckup-осмотр-в-комнате)
   7.5 [Неожиданный визит](#eventharveyroomcheckup2-неожиданный-визит)
8. [Приложение: прочие события](#appendix)

---

## Часть I. Первая встреча

<a id="часть-i-первая-встреча"></a>

<a id="eventharveyfirstmeeting-автобусная-остановка"></a>

### Автобусная остановка

*BusStop · `eventHarveyFirstMeeting`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `BusStop` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 06:00–02:00 (след. день)
  - Не просмотрено событие `eventHarveyFirstMeeting`
  - Нет топика: `topicFirstMeeting` (после первой встречи на автобусе)
  - *Сырой ключ:* `eventHarveyFirstMeeting/Time 0600 2600/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstMeeting/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicFirstMeeting`

**Харви:** Здравствуйте! Вы... это вы купили старую ферму?
*улыбается, но взгляд становится обеспокоенным* Добро пожаловать в долину...*(с улыбкой)*

**Харви:** *нерешительно* Простите, но... вы хорошо себя чувствуете? У вас очень бледный цвет лица...*(грустно)*

Я... просто устала от переезда...

**Харви:** *мягко* Понимаю, переезд - это стресс. Но вы дрожите... *снимает пиджак* *(грустно)*
Возьмите, пожалуйста. На улице довольно прохладно.*(нежно)*

Спасибо... но не обязательно...

**Харви:** *настойчиво, но мягко* Обязательно. *накидывает пиджак на плечи* *(нежно)*
Вы не забываете есть? Вы выглядите... очень хрупкой.*(грустно)*

Сегодня еще не успела поесть...

**Харви:** *обеспокоенно* У меня есть кофе и бутерброды. Я всегда беру с собой запас - привычка врача.*(с улыбкой)*

**Харви:** *протягивает контейнер* Это не лечение, просто... человеческая забота.*(нежно)*
Съешьте хотя бы половину, ладно?*(грустно)*

Вы... врач? И так заботитесь о незнакомцах?

**Харви:** *смущенно* Да, местный доктор. И... *тише* не могу пройти мимо, когда вижу, что кто-то плохо себя чувствует.*(грустно)*

**Харви:** Вы напоминаете мне... *останавливается* Неважно.*(грустно)*
Главное - вам нужно восстановиться после переезда.*(нежно)*

Кого я вам напоминаю?

**Харви:** *колеблется* Когда я работал в городской больнице... встречал людей, которые забывали заботиться о себе.*(грустно)*
*мягко* Но об этом потом. Сейчас важнее ваше самочувствие.*(нежно)*

**Выбор:**
- Согласиться поесть
- Вежливо отказаться

**Харви:** *с облегчением* Отлично. Ешьте медленно, не торопитесь.*(с улыбкой)*
А пока расскажите - как вам наша долина? Надеюсь, соседи встретили дружелюбно?*(нежно)*

Все очень добрые... но я еще толком никого не встретила...

**Харви:** *улыбается* Тогда я первый официальный сосед.*(с улыбкой)*
Если что-то понадобится - лекарства, совет, или просто поговорить - клиника всегда открыта.*(нежно)*

**Харви:** *встает* А сейчас вам лучше отдохнуть. Организм восстанавливается только во сне.*(грустно)*
*мягко* Завтра зайдите в клинику, я дам вам витамины. Для адаптации.*(нежно)*

Спасибо... вы очень добрый...

**Харви:** *смущенно краснеет* Просто... берегите себя, хорошо?*(грустно)*
*тихо* Увидимся завтра.*(нежно)*

**Харви:** Рад был познакомиться. Отдыхайте хорошо.*(нежно)* *(конец сцены)*

---

<a id="eventharveycheckup-первый-осмотр"></a>

### Первый осмотр

*Hospital · `eventHarveyCheckup`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Hospital` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 14:00–16:00
  - Активен разговорный топик: `topicAgreedCheckup` (согласие на осмотр (fork первой встречи))
  - Просмотрено событие `eventHarveyFirstMeeting`
  - Прошло дней в сохранении ≥ 2
  - *Сырой ключ:* `eventHarveyCheckup/Time 1400 1600/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicAgreedCheckup/GameStateQuery PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstMeeting/GameStateQuery DAYS_PLAYED 2`

**Харви:** *поднимает голову от бумаг* А, вы пришли! *улыбается* Проходите, располагайтесь.*(с улыбкой)*
*указывает на кушетку* Садитесь, пожалуйста. Это займет совсем немного времени.*(нежно)*

**Харви:** *моет руки* Итак, как вы себя чувствуете? Есть жалобы?*(грустно)*

Вроде все нормально...

**Харви:** *кивает* Хорошо. Тем не менее, давайте проверим основные показатели.*(с улыбкой)*
*достает стетоскоп* Можно послушать сердце?*(грустно)*

Конечно...

**Харви:** *осторожно прикладывает стетоскоп* Дышите глубоко... Хорошо.*(нежно)*
*убирает стетоскоп* Ритм нормальный, хотя немного учащенный. Это от волнения?*(грустно)*

Наверное...

**Харви:** *мягко улыбается* Понимаю. Многие так себя чувствуют.*(нежно)*
Но видите - ничего страшного. *измеряет давление* Сейчас проверим давление.*(серьёзно)*

**Харви:** *изучает показания* Немного повышено, но в пределах нормы.*(грустно)*
*садится напротив* А теперь честно - вы регулярно питаетесь?

**Выбор:**
- Да, стараюсь
- Не всегда получается

**Харви:** *с одобрением* Замечательно. Это очень важно для восстановления.*(с улыбкой)*
Вы уже выглядите лучше, чем при знакомстве.*(нежно)*

**Харви:** Еще несколько советов - пейте больше воды, особенно в жару.*(грустно)*
И не работайте до изнеможения. Отдых тоже важен.*(нежно)*

**Харви:** *встает* Ну что ж, осмотр окончен. Все показатели в норме.*(с улыбкой)*
*мягко* Вы храбрая девушка. Не каждый решится начать новую жизнь.*(нежно)*

Иногда не чувствую себя храброй...

**Харви:** *тепло* А храбрость не в том, чтобы не бояться.*(нежно)*
Она в том, чтобы продолжать, несмотря на страх.*(нежно)*

**Харви:** *улыбается* Приходите через месяц на контрольный осмотр?*(с улыбкой)*
Или раньше, если что-то будет беспокоить.*(грустно)*

Хорошо... спасибо за заботу.

**Харви:** *краснеет* Это моя работа... и удовольствие.*(нежно)*
*провожает к выходу* Берегите себя!*(с улыбкой)*
Заходите еще! И не забывайте про витамины.*(нежно)*

---

<a id="eventharveyfirstvisit-визит-на-ферму"></a>

### Визит на ферму

*Farm · `eventHarveyFirstVisit`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Farm` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 06:00–12:00
  - Активен разговорный топик: `topicFirstMeeting` (после первой встречи на автобусе)
  - Не просмотрено событие `eventHarveyFirstVisit`
  - Прошло дней в сохранении ≥ 3
  - *Сырой ключ:* `eventHarveyFirstVisit/Time 600 1200/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicFirstMeeting/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstVisit/GameStateQuery DAYS_PLAYED 3`

**Харви:** Привет! Как дела? Просто хотел проверить, как ты себя чувствуешь.*(с улыбкой)*

Я... я в порядке, спасибо...

**Харви:** Ты выглядишь немного уставшей. Долгая дорога?*(грустно)*

Да, немного...

**Харви:** Понимаю. Если тебе понадобится помощь - я всегда рядом.*(с улыбкой)*
Но только если ты сама захочешь.*(нежно)*

**Выбор:**
- ...
- Спасибо за заботу
- Я справлюсь сама
- Мне нужно время привыкнуть(break)friendship Harvey 25speak Harvey "Рад, что ты это ценишь.*(с улыбкой)*
- Но не стесняйся говорить, если что-то не так.*(нежно)*"emote farmer 60message "Хорошо... Спасибо."speak Harvey "Не за что. Просто забочусь о своих пациентах.*(с улыбкой)*"action addConversationTopic topicHarveyFirstVisitAgree 7(break)friendship Harvey 15speak Harvey "Конечно, ты сильная.*(с улыбкой)*
- Но помни - просить о помощи не стыдно.*(нежно)*"message "Спасибо за понимание..."speak Harvey "Всегда пожалуйста.*(с улыбкой)*
- Если передумаешь - я здесь.*(нежно)*"action addConversationTopic topicHarveyFirstVisitNeutral 7(break)friendship Harvey 10speak Harvey "Конечно, я понимаю.*(грустно)*
- Каждому нужно время, чтобы привыкнуть к новому месту.*(нежно)*"message "Да, именно так..."speak Harvey "Я буду рядом, когда будешь готова.*(с улыбкой)*
- Никакого давления.*(нежно)*"action addConversationTopic topicHarveyFirstVisitRefused 7

---

<a id="eventharveysecondvisit-второй-визит-травяной-чай"></a>

### Второй визит — травяной чай

*Farm · `eventHarveySecondVisit`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Farm` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 06:00–12:00
  - Прошло дней в сохранении ≥ 7
  - Нет топика: `topicHarveyFirstVisitAgree`
  - Нет топика: `topicHarveyFirstVisitNeutral`
  - Нет топика: `topicHarveyFirstVisitRefused`
  - Просмотрено событие `eventHarveyFirstVisit`
  - Не просмотрено событие `eventHarveySecondVisit`
  - *Сырой ключ:* `eventHarveySecondVisit/Time 600 1200/GameStateQuery DAYS_PLAYED 7/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveyFirstVisitAgree/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveyFirstVisitNeutral/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveyFirstVisitRefused/GameStateQuery PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstVisit/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveySecondVisit`

**Харви:** Привет! Я принёс тебе витаминный чай. Он поможет восстановить силы.*(с улыбкой)*

Спасибо, но я не болею...

**Харви:** Конечно, ты здорова!*(с улыбкой)*
Но витамины никогда не помешают, особенно в новом месте.*(нежно)*

Харви осторожно кладёт пакетик чая ей в карман.

**Харви:** Попробуй, если захочешь. Никаких обязательств.*(с улыбкой)*

**Выбор:**
- ...
- Попробую чай
- Спасибо, но пока не хочу
- Может быть позже(break)friendship Harvey 20speak Harvey "Отлично! Надеюсь, тебе понравится.*(с улыбкой)*"message "Спасибо за заботу..."speak Harvey "Всегда пожалуйста.*(нежно)*"action addConversationTopic topicHarveySecondVisitAgree 7(break)friendship Harvey 15speak Harvey "Конечно, никаких проблем.*(с улыбкой)*
- Чай будет ждать, когда ты будешь готова.*(нежно)*"message "Спасибо за понимание..."speak Harvey "Конечно. Я не хочу давить.*(с улыбкой)*"action addConversationTopic topicHarveySecondVisitNeutral 7(break)friendship Harvey 10speak Harvey "Хорошо, я понимаю.*(грустно)*
- Каждому нужно время.*(нежно)*"message "Да, именно так..."speak Harvey "Я буду рядом, когда захочешь.*(с улыбкой)*"action addConversationTopic topicHarveySecondVisitRefused 7

---

<a id="eventharveyfirstwalk-прогулка-в-лес"></a>

### Прогулка в лес

*Farm · `eventHarveyFirstWalk`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Farm` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 06:00–12:00
  - Погода: солнечно
  - Прошло дней в сохранении ≥ 11
  - Нет топика: `topicHarveySecondVisitAgree`
  - Нет топика: `topicHarveySecondVisitNeutral`
  - Нет топика: `topicHarveySecondVisitRefused`
  - Просмотрено событие `eventHarveySecondVisit`
  - Не просмотрено событие `eventHarveyFirstWalk`
  - *Сырой ключ:* `eventHarveyFirstWalk/Time 600 1200/Weather Sunny/GameStateQuery DAYS_PLAYED 11/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveySecondVisitAgree/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveySecondVisitNeutral/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveySecondVisitRefused/GameStateQuery PLAYER_HAS_SEEN_EVENT Current eventHarveySecondVisit/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstWalk`

**Харви:** Привет. Я заметил, что ты часто бываешь в лесу.*(с улыбкой)*
Сегодня хорошая погода для прогулки.

Да... я люблю этот лес.

**Харви:** Тогда пойдём вместе — до заката. Свежий воздух полезен.*(с улыбкой)*

**Выбор:**
- Согласиться
- Отказаться

**Харви:** Понимаю. Но если передумаешь — я свободен до заката.*(грустно)*

Может быть в другой раз...

**Харви:** Конечно. Только не забывай выходить на улицу.*(грустно)*

---

## Часть II. История доверия (HarveyOverhaul Story)

<a id="часть-ii-история-доверия-harveyoverhaul-story"></a>

<a id="harveyoverhaulstory-e1-slipperypath-скользкая-дорожка"></a>

### Скользкая дорожка

*BusStop · `HarveyOverhaulStory.E1_SlipperyPath`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `BusStop` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 07:00–14:00
  - Погода: ветер
  - Не день фестиваля
  - Дружба с Harvey ≥ 2 сердечек (500 pts)
  - Не просмотрено событие `HarveyOverhaulStory.E1_SlipperyPath`
  - Нет топика: HarveyMod_CD_Global
  - *Сырой ключ:* `HarveyOverhaulStory.E1_SlipperyPath/Time 700 1400/Weather Wind/!FestivalDay/Friendship Harvey 500/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E1_SlipperyPath/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global`

Порыв ветра бросает мокрые листья под ноги.

**Харви:** Осторожно. Не двигайтесь.*(в панике)*

Он подходит — и останавливается, оставляя между вами полшага.

**Выбор:**
- Принять руку
- Отступить самой
- Сделать вид, что всё в порядке(break)emote farmer 40move Harvey -1 0 3pause 200message "Он берёт вашу ладонь — твёрдо, без лишних слов."speak Harvey "Вот так. Крепче. Я держу, пока вы не встанете устойчиво.*(в панике)*"emote farmer 56speak Harvey "Не из вежливости — из безопасности. На мокрых камнях героизм обычно заканчивается синяками.*(строго)*"(break)emote farmer 12speak Harvey "Хорошо. Не подхожу."pause 300textAboveHead Harvey "Шаг назад"speak Harvey "Тогда просто слушайте мой голос: шаг назад, ближе к ограде. Медленно.*(строго)*"pause 400move farmer -1 0 3emote farmer 40speak Harvey "Да. Вот так. Упрямая, но координация хорошая."(break)emote farmer 28speak Harvey "Убедительно. Почти.*(строго)*"pause 300speak Harvey "Но я врач, а не прохожий. Я вижу, когда человек бледнеет и задерживает дыхание.*(строго)*"pause 400move Harvey -1 0 3speak Harvey "Позвольте хотя бы пройти рядом. Я не буду держать вас, если не понадобится.

**Харви:** Вы завтракали? Можете не отвечать. По лицу я уже почти понял.*(серьёзно)*

**Выбор:**
- Кивнуть
- Покачать головой
- Отвернуть взгляд(break)emote farmer 40speak Harvey "Понял. Завтрак — потом. Сегодня хотя бы долом.*(строго)*"speak Harvey "Не спорю с фактами. Слежу за пульсом — такова жизнь.*(серьёзно)*"(break)emote farmer 12speak Harvey "Хорошо. Кивка достаточно."speak Harvey "Если я ошибся — покажите."(break)message "Ты отводишь взгляд к обочине."speak Harvey "Не хотите говорить. Вижу."speak Harvey "Достаточно того, что вы ещё на ногах.

**Харви:** Сначала устойчивый шаг, потом гордость.*(строго)*
Если закружится голова — клиника рядом. И я рядом.

Он идёт у сухого края дорожки — между вами и обочиной.

---

<a id="harveyoverhaulstory-e2-insistentexam-настойчивый-осмотр"></a>

### Настойчивый осмотр

*Hospital · `HarveyOverhaulStory.E2_InsistentExam`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Hospital` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 09:00–17:00
  - Не день фестиваля
  - Дружба с Harvey ≥ 3 сердечек (750 pts)
  - Просмотрено событие `HarveyOverhaulStory.E1_SlipperyPath`
  - Не просмотрено событие `HarveyOverhaulStory.E2_InsistentExam`
  - Нет топика: HarveyMod_CD_Global HarveyMod_CD_E1
  - *Сырой ключ:* `HarveyOverhaulStory.E2_InsistentExam/Time 0900 1700/!FestivalDay/Friendship Harvey 750/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E1_SlipperyPath/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E2_InsistentExam/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global HarveyMod_CD_E1`

**Харви:** Садитесь. Пульс, дыхание, осмотр — по порядку.*(строго)*

Ты крепче сжимаешь край кушетки.

**Харви:** Дышите ровно… ещё раз…*(серьёзно)*

**Харви:** Пульс учащён. Кожа холодная. Вы слишком долго тащите всё на себе.*(строго)*

Ты отводишь взгляд к двери.

**Харви:** Дверь.*(серьёзно)*

**Харви:** Хотите уйти? Или просто тяжело, когда смотрят в упор?

**Выбор:**
- Кивнуть
- Покачать головой
- Показать на окно(break)emote farmer 40speak Harvey "Понял. Не удерживаю."speak Harvey "Но осмотр не отменяем. Сокращаем.*(строго)*"(break)emote farmer 12speak Harvey "Хорошо. Кивка достаточно, чтобы продолжить."speak Harvey "Если я ошибся — покажите."(break)message "Ты указываешь на окно — туда, где меньше суеты."speak Harvey "Свет. Воздух. Разумно."speak Harvey "Останемся у окна. Ближе к выходу — психологически, не медицински.

**Харви:** Я вижу, что давлю. Но я не отступлю от главного: ваше состояние требует внимания.*(строго)*

**Харви:** Сейчас я скажу неприятную вещь.*(строго)*

**Харви:** Вы не обязаны становиться идеальной пациенткой за один день. Но хотя бы один пункт режима вы сегодня выберете.

**Харви:** Один. Не спорьте, это уже моя уступка.*(строго)*

Ты киваешь не сразу.

**Выбор:**
- С чего начать режим?
- Вода с собой
- Тёплый завтрак
- Вечерний отдых
- Попросить Харви решить самому(break)speak Harvey "Хорошо. Бутылка у двери. Не в сундуке, не где-то на ферме — у двери.*(строго)*
- Я зануден, знаю. Зато обезвоживание тоже занудное и куда менее обаятельное."addConversationTopic topicHarveyTrust_Water 7(break)speak Harvey "Тёплый завтрак. Даже маленький.*(строго)*
- Не пир на весь город. Каша, суп, кусок хлеба с сыром — что угодно, что не выглядит как забытое тело."addConversationTopic topicHarveyTrust_Breakfast 7(break)speak Harvey "Вечерний отдых. Хороший выбор.
- Ферма переживёт несобранный сорняк. Ваш организм — не всегда.*(строго)*"addConversationTopic topicHarveyTrust_Rest 7(break)speak Harvey "Тогда решаю как врач: вода.*(строго)*
- Самое простое. Самое скучное. Самое недооценённое.
- И да, я буду проверять. Не потому что не доверяю вам. Потому что тревожусь.*(серьёзно)*"addConversationTopic topicHarveyTrust_DoctorDecides 7

**Харви:** Вот теперь можно спорить со мной сколько угодно. Но выбранный пункт — выполняете.*(строго)*

**Харви:** Я рядом. И если придётся быть настойчивым — я буду.

**Харви:** Выпейте, пока горячий. Я посмотрю на цвет лица — и отпущу.*(серьёзно)*

Ты берёшь чашку обеими руками.

**Харви:** Кивок засчитан. И, кажется, чай тоже.*(с улыбкой)*

**Харви:** До двери — медленно. Завтра пришлю время встречи на пирсе.

**Харви:** Завтра — пирс, после шести. Я буду ждать. *(конец сцены)*

---

<a id="harveyoverhaulstory-e3-forestapothecary-лесная-аптека"></a>

### Лесная аптека

*Forest · `HarveyOverhaulStory.E3_ForestApothecary`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Forest` при выполнении условий ниже
- **CP preconditions:**
  - День недели: чт, пт, сб
  - Время суток: 12:00–18:00
  - Не день фестиваля
  - Погода: солнечно
  - Дружба с Harvey ≥ 4 сердечек (1000 pts)
  - Просмотрено событие `HarveyOverhaulStory.E2B_QuietAgreement`
  - Не просмотрено событие `HarveyOverhaulStory.E3_ForestApothecary`
  - Нет топика: HarveyMod_CD_Global
  - Нет топика: HarveyMod_CD_E2
  - *Сырой ключ:* `HarveyOverhaulStory.E3_ForestApothecary/DayOfWeek Thu Fri Sat/Time 1200 1800/!FestivalDay/Weather Sunny/Friendship Harvey 1000/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E2B_QuietAgreement/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E3_ForestApothecary/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E2`

**Харви:** Вы обещали показать лес. Я буду тихим — обещаю.*(с улыбкой)*

Ты осторожно отделяешь один стебель от другого и кладёшь его поверх корзины.

Харви следит за твоими руками внимательнее, чем за тропой.

**Харви:** Ножницы — вам. Корзину — мне. Высокие ветки — на потом, если понадобится.

**Выбор:**
- Что показать Харви?
- Лечебную траву
- Ядовитое растение
- Мох у корней
- Просто протянуть корзину(break)message "Ты наклоняешься к низкому кусту и показываешь на листья — два пальца, аккуратно."emote farmer 32speak Harvey "Ага. Это вы берёте для чая?"speak Harvey "Не улыбайтесь так загадочно. Я врач, но сейчас чувствую себя студентом на экзамене.*(с улыбкой)*"speak Harvey "Хорошо. Запоминаю: не рвать с корнем."(break)message "Ты останавливаешь его жестом — ладонь вперёд, без слов."speak Harvey "О. Вот это выражение лица я знаю. Это выражение «доктор, не трогайте»."speak Harvey "Принято. Руки держу при себе."speak Harvey "Видите? Я обучаемый. Особенно когда на кону дерматит.*(с улыбкой)*"(break)message "Ты проводишь пальцем по мягкому моху у корней — медленно, будто по карте."speak Harvey "Мох?"speak Harvey "Вы смотрите на него так, будто это маленькая палата интенсивной терапии для леса.*(нежно)*"speak Harvey "Влажность, тень, покой… да. Понимаю. Иногда лучшие лекарства выглядят очень тихо."(break)message "Ты протягиваешь корзину — молча, без объяснений."speak Harvey "Понял. Моя медицинская квалификация сегодня: носить корзину.*(с улыбкой)*"speak Harvey "Честная работа. Спина у меня крепкая, а у вас — нет, не спорьте, я видел, как вы потянулись к ветке.*(строго)*

Ты киваешь — коротко, без слов.

**Харви:** Вы почти ничего не сказали, а я узнал больше, чем за половину медицинских лекций.*(нежно)*

**Харви:** Только одно условие: высокие ветки — мои. Это не контроль, это здравый смысл и мой рост наконец-то приносит пользу.*(с улыбкой)*

---

<a id="harveyoverhaulstory-e4-pierbreath-дыхание-у-пирса"></a>

### Дыхание у пирса

*Beach · `HarveyOverhaulStory.E4_PierBreath`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Beach` при выполнении условий ниже
- **CP preconditions:**
  - Погода: солнечно
  - Время суток: 18:00–02:00 (след. день)
  - Не день фестиваля
  - Дружба с Harvey ≥ 5 сердечек (1250 pts)
  - Просмотрено событие `HarveyOverhaulStory.E3B_WingPatient`
  - Не просмотрено событие `HarveyOverhaulStory.E4_PierBreath`
  - Нет топика: HarveyMod_CD_Global
  - *Сырой ключ:* `HarveyOverhaulStory.E4_PierBreath/Weather Sunny/Time 1800 2600/!FestivalDay/Friendship Harvey 1250/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E3B_WingPatient/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E4_PierBreath/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global`

**Харви:** На воде дыхание честнее. Его труднее обмануть.

**Харви:** Встаньте рядом. Не близко, если не хотите. Но так, чтобы слышать мой счёт.

Харви вдыхает с волной — ты ловишь ритм ушами, не глазами.

Плечи опускаются сами, будто отпустили невидимую верёвку.

**Харви:** Если стало легче — сожмите мою руку один раз.
Если нет — два.
Если не хотите отвечать — просто стойте. Я пойму и это.

Он протягивает ладонь — не настаивая, но держит её открытой.

**Выбор:**
- Сжать один раз
- Сжать два раза
- Не двигаться
- Отпустить руку(break)message "Ты один раз сжимаешь его ладонь."speak Harvey "Хорошо. Значит, волны сегодня работают лучше моих лекций.*(нежно)*"speak Harvey "Запомните это чувство. Не меня — ритм."addConversationTopic topicHarveyTrust_TouchOk 7(break)message "Ты сжимаешь его ладонь два раза — коротко, по делу."speak Harvey "Понял. Тогда не заставляем дыхание быть правильным."speak Harvey "Иногда организм сначала спорит. Ничего. Я умею быть терпеливым.*(серьёзно)*"addConversationTopic topicHarveyTrust_BreathHard 7(break)message "Ты стоишь неподвижно, глядя на воду."speak Harvey "Ответа нет. Это тоже ответ."speak Harvey "Я останусь рядом и помолчу. Редкий медицинский метод, но действенный.*(нежно)*"addConversationTopic topicHarveyTrust_NeedsSpace 7(break)message "Ты медленно отпускаешь его руку."speak Harvey "Хорошо. Отпускаю."speak Harvey "Счёт останется. Рука — только если понадобится."addConversationTopic topicHarveyTrust_NeedsSpace 7

**Харви:** У вас получается. Не потому что вы послушная пациентка.

**Харви:** Потому что вы всё ещё здесь.*(нежно)*

**Харви:** Домой я провожу. Это уже не обсуждение, а прогноз погоды: темнеет, дорожки скользкие, а я тревожный врач.*(с улыбкой)*

---

<a id="harveyoverhaulstory-e5-stormbeside-рядом-в-грозу"></a>

### Рядом в грозу

*Hospital · `HarveyOverhaulStory.E5_StormBeside`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Hospital` при выполнении условий ниже
- **CP preconditions:**
  - Погода: гроза
  - Время суток: 14:00–20:00
  - Не день фестиваля
  - Дружба с Harvey ≥ 6 сердечек (1500 pts)
  - Просмотрено событие `HarveyOverhaulStory.E4B_TooQuiet`; Не просмотрено событие `HarveyOverhaulStory.E5_StormBeside`; Нет топика: HarveyMod_CD_Global; Нет топика: HarveyMod_CD_E4B
  - *Сырой ключ:* `HarveyOverhaulStory.E5_StormBeside/Weather Storm/Time 1400 2000/!FestivalDay/Friendship Harvey 1500/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E4B_TooQuiet, !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E5_StormBeside, !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global, !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E4B`

**Харви:** Ко мне. Сейчас — внутрь.*(в панике)*

**Харви:** Да, я строгий. Гроза — не время доказывать самостоятельность.*(строго)*

**Харви:** Рукам — работу. Панике не оставляем свободного места.*(строго)*

Он вкладывает в ладонь моток марли.

Ты перебираешь бинты медленно, пока пальцы не перестают дрожать.

Харви следит не за бинтами, а за твоим дыханием.

**Харви:** Составим план. Не красивый. Рабочий.*(строго)*

**Харви:** Если гром станет слишком сильным, что вы сделаете?

**Выбор:**
- Прийти в клинику
- Остаться дома
- Оставить записку
- Попросить Харви проводить(break)speak Harvey "Правильно."speak Harvey "Я оставлю свет у окна. Если дверь закрыта — стучите. Если не можете стучать — стойте под навесом, я проверю.*(строго)*"friendship Harvey 45addConversationTopic topicHarveyStorm_Clinic 7(break)speak Harvey "Принимается. Но тогда дома: плед, вода, лампа, дверь не запирать на засов.*(строго)*"speak Harvey "Да, я перечисляю. Да, вы закатываете глаза. Мы оба справляемся как умеем."friendship Harvey 40addConversationTopic topicHarveyStorm_Home 7(break)speak Harvey "Записка подойдёт."speak Harvey "Короткая. Хоть одно слово. Хоть крестик.*(строго)*"speak Harvey "Я пойму."friendship Harvey 35addConversationTopic topicHarveyStorm_Note 7(break)speak Harvey "Конечно."speak Harvey "И прежде чем вы решите, что это слишком — нет. Попросить сопровождение в грозу разумно.*(строго)*"speak Harvey "Я даже не буду делать вид, что мне это не спокойнее.*(серьёзно)*"friendship Harvey 50addConversationTopic topicHarveyStorm_Escort 7

**Харви:** Вот теперь хорошо. Не потому что страх исчез.

**Харви:** А потому что у страха появился маршрут, время и запасной выход.*(строго)*

**Харви:** Это и есть медицина: не обещать чудо, а снижать риск.

Он ставит чай на край стола и остаётся рядом, пока не стихнет последний раскат.

---

<a id="harveyoverhaulstory-e6-sayitoutloud-сказать-вслух"></a>

### Сказать вслух

*Hospital · `HarveyOverhaulStory.E6_SayItOutLoud`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Hospital` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 19:00–23:30
  - Не день фестиваля
  - Дружба с Harvey ≥ 7 сердечек (1750 pts)
  - Просмотрено событие `HarveyOverhaulStory.E5_StormBeside`; Не просмотрено событие `HarveyOverhaulStory.E6_SayItOutLoud`; Нет топика: HarveyMod_CD_Global; Нет топика: HarveyMod_CD_E5
  - *Сырой ключ:* `HarveyOverhaulStory.E6_SayItOutLoud/Time 1900 2330/!FestivalDay/Friendship Harvey 1750/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E5_StormBeside, !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E6_SayItOutLoud, !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global, !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E5`

Харви закрывает журнал и откладывает карту на край стола.

**Харви:** Я хочу кое-что сказать вслух. Так меньше шансов, что я спрячу это за медицинскими словами.

**Харви:** Я врач. Это не заканчивается, когда клиника закрывается.

**Харви:** Если я вижу риск — я вмешиваюсь. Иногда раньше, чем человек успевает попросить.*(строго)*

**Харви:** С вами это стало… заметнее.*(серьёзно)*

**Харви:** Я забочусь. И иногда забочусь слишком сильно.

**Харви:** Но я не хочу, чтобы моя забота стала клеткой.*(строго)*

**Выбор:**
- Как ответить?
- Кивнуть
- Сделать шаг ближе
- Сделать шаг назад
- Посмотреть на дверь(break)emote farmer 40speak Harvey "Спасибо."speak Harvey "Тогда я скажу правило: я рядом. Не впереди вас. Не вместо вас. Рядом.*(нежно)*"speak Harvey "За исключением случаев, когда вы падаете, истекаете кровью или идёте в шахту с температурой. Там я снова становлюсь невыносимым.*(строго)*"friendship Harvey 50(break)message "Ты делаешь шаг ближе."move farmer -1 0 3speak Harvey "Вот так?"speak Harvey "Хорошо. Я не буду двигаться. Вы сами выбрали расстояние.*(нежно)*"speak Harvey "Запомню. Это важно."friendship Harvey 55(break)message "Ты делаешь шаг назад."move farmer 1 0 1speak Harvey "Понял. Больше воздуха."speak Harvey "Я могу быть настойчивым и отсюда. Проверено медицинской практикой.*(строго)*"speak Harvey "Но спасибо, что показали."friendship Harvey 45(break)faceDirection farmer 0message "Ты смотришь на дверь."speak Harvey "Хотите уйти?"speak Harvey "Хорошо. Разговор не должен быть ловушкой."speak Harvey "Я провожу до выхода. Только до выхода. Дальше — как выберете."friendship Harvey 40

**Харви:** Я всё равно буду напоминать про еду, воду и отдых.*(строго)*

**Харви:** Вы всё равно будете делать вид, что не слышите.

**Харви:** Но теперь между нами есть договор: я спрашиваю, когда могу. Вы показываете, когда слов нет.*(нежно)*

**Харви:** Для начала этого достаточно.

---

<a id="harveyoverhaulstory-e7-townsip-sunny-глоток-солнца-в-городе"></a>

### Глоток солнца в городе

*Town · `HarveyOverhaulStory.E7_TownSip_Sunny`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Town` при выполнении условий ниже
- **CP preconditions:**
  - Погода: солнечно
  - Время суток: 12:00–15:00
  - Не день фестиваля
  - Дружба с Harvey ≥ 8 сердечек (2000 pts)
  - Просмотрено событие `HarveyOverhaulStory.E6_SayItOutLoud`
  - Не просмотрено событие `HarveyOverhaulStory.E7_TownSip_Sunny`
  - Нет топика: HarveyMod_CD_Global
  - *Сырой ключ:* `HarveyOverhaulStory.E7_TownSip_Sunny/Weather Sunny/Time 1200 1500/!FestivalDay/Friendship Harvey 2000/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E6_SayItOutLoud/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E7_TownSip_Sunny/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global`

Солнце греет камни на площади. На секунду шум города становится слишком густым.

**Харви:** Вы остановились.*(серьёзно)*

**Харви:** Нет, не отвечайте. Я вижу.*(строго)*

**Харви:** Я очень хочу сейчас прочитать лекцию о воде и завтраке.*(строго)*

**Харви:** Но мы договаривались: помощь не должна становиться спектаклем.

Харви ставит бутылку на край лавки — не протягивая её.

**Выбор:**
- Взять бутылку
- Сесть на лавку
- Отойти к дереву
- Сделать вид, что всё нормально(break)message "Ты берёшь бутылку с края лавки."speak Harvey "Спасибо."speak Harvey "Я буду считать это зрелым медицинским сотрудничеством, а не моей победой."speak Harvey "Хотя… немного моей победой.*(с улыбкой)*"(break)message "Ты садишься на лавку, опустив плечи."showFrame farmer 107speak Harvey "Хорошо. Сидим."speak Harvey "Я рядом. Не нависаю, не проверяю пульс посреди площади, горжусь собой.*(с улыбкой)*"(break)message "Ты отходишь к дереву у края площади."move farmer -2 0 3move Harvey -2 0 3speak Harvey "Тень. Верно."speak Harvey "Идём. Медленно. Я буду рядом, но не буду держать, пока не понадобится."(break)emote farmer 28speak Harvey "Удивительно убедительно."speak Harvey "Почти поверил бы, если бы не был врачом и занудой.*(с улыбкой)*"speak Harvey "Бутылка останется здесь. Совершенно случайная бутылка. Городская архитектура.

**Penny:** Добрый день, доктор.

**Харви:** Добрый, Пенни.

**Харви:** Видите? Никто не обязан знать, что вам стало нехорошо.

**Харви:** Я всё ещё хочу проводить вас домой.*(строго)*

**Харви:** Но сначала спрошу: домой, в тень или в клинику?

**Выбор:**
- Домой
- В тень
- В клинику(break)speak Harvey "Хорошо. Светлыми улицами."move farmer 0 2 0move Harvey 0 2 0(break)speak Harvey "Пять минут. Потом решим снова."message "Ты садишься в тень у стены — Харви стоит чуть в стороне."(break)speak Harvey "Разумно. Очень разумно."speak Harvey "Идём. Без спешки.*(строго)*"move Harvey -3 0 3move farmer -3 0 3

---

<a id="harveyoverhaulstory-e8-quietshelf-тихая-полка"></a>

### Тихая полка

*ArchaeologyHouse · `HarveyOverhaulStory.E8_QuietShelf`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `ArchaeologyHouse` при выполнении условий ниже
- **CP preconditions:**
  - День недели: сб
  - Время суток: 10:00–16:00
  - Не день фестиваля
  - Дружба с Harvey ≥ 8 сердечек (2000 pts)
  - Просмотрено событие `HarveyOverhaulStory.E7_TownSip_Sunny`
  - Не просмотрено событие `HarveyOverhaulStory.E8_QuietShelf`
  - Нет топика: HarveyMod_CD_Global
  - Нет топика: HarveyMod_CD_E7
  - *Сырой ключ:* `HarveyOverhaulStory.E8_QuietShelf/DayOfWeek Sat/Time 1000 1600/!FestivalDay/Friendship Harvey 2000/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E7_TownSip_Sunny/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E8_QuietShelf/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E7`

**Gunther:** Здравствуйте, фермер. Поможете с карточками каталога?*(с улыбкой)*
Разложите их по видам — и если достанете коробку с верхней полки, будет проще. Только тихо, пожалуйста. Здесь всё должно быть в порядке.

Ты берёшь стопку карточек и смотришь на полку.

Коробка стоит выше, чем казалось сначала.

**Gunther:** Я сейчас принесу ещё одну коробку из дальнего зала. Не торопитесь.

Ты протягиваешь руку к верхней полке.

**Харви:** Я хочу сказать «не трогайте, я сам». Очень хочу.*(строго)*

**Харви:** Но мы, кажется, тренируем другой навык.

**Харви:** Варианты: я достаю коробку, я страхую, или я стою рядом и молча страдаю.

**Выбор:**
- Попросить Харви достать коробку
- Попросить подстраховать
- Попробовать самой
- Показать на нижние карточки(break)speak Harvey "С радостью."speak Harvey "Спасибо, что попросили, а не полезли молча. Это сэкономило мне два седых волоса.*(с улыбкой)*"move Harvey 2 0 1pause 300message "Он легко снимает коробку и ставит её на стол."move Harvey -2 0 3friendship Harvey 30addConversationTopic topicHarveyHelp_Asks 7(break)speak Harvey "Хорошо. Я рядом."speak Harvey "Не вместо вас. Рядом."speak Harvey "Если лестница качнётся, я вмешаюсь без демократического голосования.*(строго)*"move Harvey 1 0 1message "Ты тянешься к коробке — Харви стоит в полушаге, готовый подхватить."friendship Harvey 28addConversationTopic topicHarveyHelp_Spotter 7(break)speak Harvey "Ладно.*(строго)*"speak Harvey "Я буду стоять здесь и героически бороться с медицинским инстинктом."message "Харви держит руки наготове, но не касается тебя."pause 500speak Harvey "Отлично. Медленно вниз. Да. Теперь я снова могу дышать.*(нежно)*"friendship Harvey 25addConversationTopic topicHarveyHelp_Independent 7(break)message "Ты указываешь на нижние карточки на столе."speak Harvey "Разумное распределение труда."speak Harvey "Вы — карточки. Я — верхние полки. Gunther — загадочный человек, который хранит тяжёлое слишком высоко.*(с улыбкой)*"friendship Harvey 32

Ты сортируешь карточки, стараясь не шуметь. Бумага шелестит, как сухие листья.

**Gunther:** Спасибо, фермер. И вам, доктор — каталог снова в порядке.*(с улыбкой)*

**Харви:** Знаете, это оказалось сложнее, чем я думал.*(серьёзно)*

**Харви:** Не помогать без спроса.

**Харви:** Но если вы решите тащить ящик с минералами одна — я сорвусь. Предупреждаю честно.*(строго)*

**Харви:** Я врач. У нас профессиональная аллергия на бессмысленный риск.

В тишине музея это звучит не как приказ, а как обещание быть рядом.

---

## Часть III. Лечение и клиника

<a id="часть-iii-лечение-и-клиника"></a>

<a id="harveymod-firsttreatment-первое-серьёзное-лечение"></a>

### Первое серьёзное лечение

*Hospital · `HarveyMod_FirstTreatment`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Hospital` при выполнении условий ниже
- **CP preconditions:**
  - Дружба с Harvey ≥ 3 сердечек (750 pts)
  - Время суток: 09:00–21:00
  - Не просмотрено событие `HarveyMod_FirstTreatment`
  - Нет топика: `topicFirstTreatmentComplete`
  - Активен разговорный топик: `topicHarveyNeedsFirstTreatment`
  - *Сырой ключ:* `HarveyMod_FirstTreatment/Friendship Harvey 750/Time 900 2100/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_FirstTreatment/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicFirstTreatmentComplete/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveyNeedsFirstTreatment`

**Харви:** Наконец-то... Я так волновался за тебя.
Садись сюда. Нужно провести полное обследование.*(серьёзно)*

**Харви:** Покажи руки. Посмотри на свет.*(серьёзно)*
*осторожно проверяет пульс* Твоё сердце бьётся слишком быстро...*(грустно)*

**Харви:** Эй, всё хорошо. Я рядом.
Я сделаю всё, чтобы с тобой ничего плохого не случилось.
Доверься мне, фермер.

**Харви:** С сегодняшнего дня - никаких походов в шахты сначала покажись мне!*(строго)*
И каждый вечер - обязательный осмотр здесь, в клинике.*(серьёзно)*
Я очень прошу отнестись к этому серьёзно.*(строго)*

**Харви:** Я знаю, ты сильная... но позволь мне заботиться о тебе.
*протягивает руку, но ждёт твоего ответа* Ты слишком важна для меня — как пациентка.

---

<a id="harveymod-nightcrisis-dating-ночной-кризис-dating-married"></a>

### Ночной кризис (dating/married)

*Hospital · `HarveyMod_NightCrisis_Dating`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Hospital` при выполнении условий ниже
- **CP preconditions:**
  - Дружба с Harvey ≥ 6 сердечек (1500 pts)
  - Время суток: 22:00–02:00 (след. день)
  - Просмотрено событие `HarveyMod_FirstTreatment`
  - Отношения с Harvey: Dating или Married
  - Не просмотрено событие `HarveyMod_NightCrisis_Dating`
  - Не просмотрено событие `HarveyMod_NightCrisis_PreDating`
  - Не просмотрено событие `HarveyMod_NightCrisis`
  - Нет топика: `topicNightCrisisComplete`
  - *Сырой ключ:* `HarveyMod_NightCrisis_Dating/Friendship Harvey 1500/Time 2200 2600/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyMod_FirstTreatment/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_NightCrisis_Dating/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_NightCrisis_PreDating/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_NightCrisis/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicNightCrisisComplete`

**Харви:** Что?! Ты здесь в такое время?!*(в панике)*
*подбегает* С тобой всё в порядке? Говоря со мной.*(в панике)*

**Харви:** Ты дрожишь... и такая бледная...*(грустно)*
*обнимает и усаживает в кресло* Я здесь. Расскажи — что мучает тебя по ночам?*(нежно)*

**Харви:** Кошмары? Панические атаки?*(грустно)*
*достаёт стетоскоп* Дыши глубоко. Слушай только меня.*(серьёзно)*

**Харви:** Вот так... пульс выравнивается.*(нежно)*
Ты в безопасности. Я никуда не уйду.*(нежно)*

**Харви:** Останься здесь до утра — тебе нужен покой.*(нежно)*
*приносит плед и подушку* Если согласна, я буду рядом всю ночь.*(нежно)*

---

<a id="harveymod-nightcrisis-predating-ночной-кризис-до-dating"></a>

### Ночной кризис (до dating)

*Hospital · `HarveyMod_NightCrisis_PreDating`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Hospital` при выполнении условий ниже
- **CP preconditions:**
  - Дружба с Harvey ≥ 6 сердечек (1500 pts)
  - Время суток: 22:00–02:00 (след. день)
  - Просмотрено событие `HarveyMod_FirstTreatment`
  - !PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married
  - Не просмотрено событие `HarveyMod_NightCrisis_PreDating`
  - Не просмотрено событие `HarveyMod_NightCrisis_Dating`
  - Не просмотрено событие `HarveyMod_NightCrisis`
  - Нет топика: `topicNightCrisisComplete`
  - *Сырой ключ:* `HarveyMod_NightCrisis_PreDating/Friendship Harvey 1500/Time 2200 2600/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyMod_FirstTreatment/GameStateQuery !PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_NightCrisis_PreDating/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_NightCrisis_Dating/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_NightCrisis/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicNightCrisisComplete`

**Харви:** Что?! Ты здесь в такое время?!*(в панике)*
*быстро подходит* С тобой всё в порядке? Что случилось?*(в панике)*

**Харви:** Ты дрожишь... и бледная...*(грустно)*
Садись, пожалуйста. Расскажи — что мучает тебя по ночам?*(серьёзно)*

**Харви:** Кошмары? Панические атаки?*(грустно)*
*достаёт стетоскоп* Дыши глубоко. Слушай мои указания.*(серьёзно)*

**Харви:** Вот так. Сердцебиение стабилизируется.
Ты в безопасности.*(серьёзно)*

**Харви:** Я останусь рядом как врач, пока состояние не стабилизируется.*(серьёзно)*
*приносит плед* Пожалуйста, останься здесь до утра. Мне нужно убедиться, что кризис не повторится.*(строго)*

---

<a id="harveymod-birthdayhospital-dating-день-рождения-в-больнице-dating"></a>

### День рождения в больнице (dating)

*Hospital · `HarveyMod_BirthdayHospital_Dating`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Hospital` при выполнении условий ниже
- **CP preconditions:**
  - Дружба с Harvey ≥ 8 сердечек (2000 pts)
  - Игрок в локации `Hospital`
  - Сезон: summer
  - Число месяца: 9
  - Отношения с Harvey: Dating или Married
  - Не просмотрено событие `HarveyMod_BirthdayHospital_Dating`
  - Не просмотрено событие `HarveyMod_BirthdayHospital_Friend`
  - Не просмотрено событие `HarveyMod_BirthdayHospital`
  - Нет топика: `topicBirthdayHospitalComplete`
  - *Сырой ключ:* `HarveyMod_BirthdayHospital_Dating/Friendship Harvey 2000/LocationName Hospital/Season summer/Day 9/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_BirthdayHospital_Dating/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_BirthdayHospital_Friend/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_BirthdayHospital/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicBirthdayHospitalComplete`

**Харви:** С днём рождения, фермер! *широко улыбается*
*(с улыбкой)*
Я знаю, ты не хотела праздновать...*(нежно)*

**Харви:** Но я не мог позволить этому дню пройти незамеченным.*(нежно)*
*достаёт небольшую коробочку* Это специально для тебя.*(с улыбкой)*

**Харви:** Энергетический кристалл. Для защиты.*(нежно)*
Носи его с собой. Пусть напоминает, что ты не одна.*(нежно)*

**Харви:** Я знаю, тебе тяжело... день рождения в больнице...*(грустно)*
Но для меня это самый важный день в году. День, когда появился человек, без которого мне уже трудно представить жизнь.*(нежно)*

**Харви:** А теперь — особенный ужин!*(с улыбкой)*
Я попросил Гаса приготовить твоё любимое блюдо. Сегодня без процедур — только мы.*(нежно)*

---

<a id="harveymod-birthdayhospital-friend-день-рождения-в-больнице-друг"></a>

### День рождения в больнице (друг)

*Hospital · `HarveyMod_BirthdayHospital_Friend`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Hospital` при выполнении условий ниже
- **CP preconditions:**
  - Дружба с Harvey ≥ 8 сердечек (2000 pts)
  - Игрок в локации `Hospital`
  - Сезон: summer
  - Число месяца: 9
  - !PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married
  - Не просмотрено событие `HarveyMod_BirthdayHospital_Friend`
  - Не просмотрено событие `HarveyMod_BirthdayHospital_Dating`
  - Не просмотрено событие `HarveyMod_BirthdayHospital`
  - Нет топика: `topicBirthdayHospitalComplete`
  - *Сырой ключ:* `HarveyMod_BirthdayHospital_Friend/Friendship Harvey 2000/LocationName Hospital/Season summer/Day 9/GameStateQuery !PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_BirthdayHospital_Friend/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_BirthdayHospital_Dating/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_BirthdayHospital/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicBirthdayHospitalComplete`

**Харви:** С днём рождения, фермер.
Я подумал, день в больнице не должен быть совсем серым.*(с улыбкой)*

**Харви:** *достаёт небольшую коробочку* Это маленький подарок. Не как врачебное назначение, обещаю.*(с улыбкой)*

**Харви:** Энергетический кристалл. Для защиты.
Ты мне очень дорога, и я хотел немного поддержать тебя.*(с улыбкой)*

**Харви:** Знаю, праздновать здесь непросто...*(грустно)*
Но хотя бы один день в году я хотел сделать для тебя чуть теплее.*(с улыбкой)*

**Харви:** Я попросил Гаса приготовить что-то простое и лёгкое.*(с улыбкой)*
Сегодня без процедур — только отдых.*(с улыбкой)*

---

<a id="harveymod-treatmentplanmeeting-план-лечения"></a>

### План лечения

*Hospital · `HarveyMod_TreatmentPlanMeeting`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Hospital` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 09:00–17:00
  - Активен разговорный топик: `topicDiagnosisComplete` (завершена диагностика (HarveyMod))
  - Дружба с Harvey ≥ 3 сердечек (750 pts)
  - Не просмотрено событие `HarveyMod_TreatmentPlanMeeting`
  - *Сырой ключ:* `HarveyMod_TreatmentPlanMeeting/Time 900 1700/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicDiagnosisComplete/Friendship Harvey 750/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_TreatmentPlanMeeting`

**Харви:** Наконец-то у меня есть полная картина твоего состояния.*(серьёзно)*
Садись, обсудим план лечения.

**Харви:** У тебя комплексное расстройство: хронический стресс с элементами тревожности.*(строго)*
Лечение будет поэтапным.

**Харви:** Этап 1: Стабилизация режима сна и отдыха
Этап 2: Работа с тревожностью
Этап 3: Восстановление эмоционального баланса.

**Харви:** Каждый этап займёт 1-2 недели. Никаких поблажек!*(строго)*
Твоё здоровье важнее любых планов.*(строго)*

**Выбор:**
- Согласиться на полный курс лечения
- Попросить сократить лечение
- Выразить сомнения в необходимости(break)speak Harvey "Отлично! Я знал, что могу на тебя положиться.*(с улыбкой)*"speak Harvey "Лечение начинается завтра. Будь готова к изменениям в образе жизни."addConversationTopic topicTreatmentAgreement 30friendship Harvey 50action removeConversationTopic topicDiagnosisComplete(break)speak Harvey "Сократить? Ты серьёзно?*(строго)*
- Это минимально необходимый курс. Меньше - и рецидив гарантирован.*(строго)*"speak Harvey "Но... хорошо. Попробуем интенсивную терапию. Но при первых признаках ухудшения - полный курс!*(строго)*"addConversationTopic topicIntensiveTreatment 21friendship Harvey 25action removeConversationTopic topicDiagnosisComplete(break)speak Harvey "Сомнения? После всего, что я видел?*(строго)*
- Твоё состояние может привести к серьёзным последствиям!*(строго)*
- Но я не могу лечить тебя против воли. Подумай ещё раз.*(грустно)*"addConversationTopic topicTreatmentRefusal 14friendship Harvey -25action removeConversationTopic topicDiagnosisComplete

---

<a id="eventharveymedicalcheck-медосмотр-по-напоминанию-pre-dating"></a>

### Медосмотр по напоминанию (pre-dating)

*Hospital · `eventHarveyMedicalCheck`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Hospital` при выполнении условий ниже
- **CP preconditions:**
  - Дружба с Harvey ≥ 6 сердечек (1500 pts)
  - Время суток: 14:00–18:00
  - Погода: солнечно
  - Получено письмо `mailHarveyMedicalCheckReminder`
  - !PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married
  - *Сырой ключ:* `eventHarveyMedicalCheck/Friendship Harvey 1500/Time 1400 1800/Weather sunny/GameStateQuery PLAYER_HAS_MAIL Current mailHarveyMedicalCheckReminder Received/GameStateQuery !PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married`

**Харви:** Привет. Рад, что ты ${пришёл^пришла}$.*(с улыбкой)*

**Харви:** Ты дрожишь. Давай сядем.

**Харви:** Сначала просто подыши... вот так.*(серьёзно)*

**Харви:** Ты похудела. Когда последний раз ела?*(строго)*

**Выбор:**
- Харви с беспокойством смотрит на тебя...
- Вчера...
- Не помню...(break)speak Harvey "Я приготовил питательный коктейль.*(строго)*"addItem 403 1(break)speak Harvey "Это уже клинический случай.*(строго)*"pause 800speak Harvey "Теперь давление... будь осторожна, манжета может напомнить тебе о..."pause 300emote farmer 28pause 500speak Harvey "Прости. Я нашел специальную детскую манжету - она мягче."pause 1200speak Harvey "110

**Харви:** Ты становишься сильнее с каждым днём... Пока постарайся не доводить себя до такого состояния. Если что-то беспокоит — приходи сразу. Я помогу. *(конец сцены)*

---

<a id="eventharveymedicalcheck-dating-медосмотр-по-напоминанию-dating"></a>

### Медосмотр по напоминанию (dating)

*Hospital · `eventHarveyMedicalCheck_Dating`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Hospital` при выполнении условий ниже
- **CP preconditions:**
  - Дружба с Harvey ≥ 6 сердечек (1500 pts)
  - Время суток: 14:00–18:00
  - Погода: солнечно
  - Получено письмо `mailHarveyMedicalCheckReminder`
  - Отношения с Harvey: Dating или Married
  - *Сырой ключ:* `eventHarveyMedicalCheck_Dating/Friendship Harvey 1500/Time 1400 1800/Weather sunny/GameStateQuery PLAYER_HAS_MAIL Current mailHarveyMedicalCheckReminder Received/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married`

**Харви:** Привет, фермер. Рад тебя видеть.*(нежно)*

**Харви:** Ты дрожишь. Давай сядем.

**Харви:** Сначала просто подыши... вот так.*(серьёзно)*

**Харви:** Ты похудела. Когда последний раз ела?*(строго)*

**Выбор:**
- Харви с беспокойством смотрит на тебя...
- Вчера...
- Не помню...(break)speak Harvey "Я приготовил питательный коктейль.*(строго)*"addItem 403 1(break)speak Harvey "Это уже клинический случай.*(строго)*"pause 800speak Harvey "Теперь давление... будь осторожна, манжета может напомнить тебе о..."pause 300emote farmer 28pause 500speak Harvey "Прости. Я нашел специальную детскую манжету - она мягче."pause 1200speak Harvey "110

**Харви:** Ты становишься сильнее с каждым днём... Я рядом, если понадоблюсь.*(нежно)* *(конец сцены)*

---

<a id="eventharveytraumaexam-осмотр-старых-шрамов"></a>

### Осмотр старых шрамов

*Hospital · `eventHarveyTraumaExam`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Hospital` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 08:00–18:00
  - Дружба с Harvey ≥ 8 сердечек (2000 pts)
  - *Сырой ключ:* `eventHarveyTraumaExam/Time 0800 1800/Friendship Harvey 2000`

**Харви:** Одну минуту... Ты дышишь неровно. Мне нужно проверить твои рёбра.*(серьёзно)*

Нет, всё в порядке...

**Харви:** Это приказ, а не просьба. *берёт стетоскоп* *(строго)*

**Харви:** Боже... У тебя... *замолкает* *(в панике)*

**Харви:** Эти шрамы... Готоранские лагеря?*(грустно)*

...

**Харви:** *осторожно накладывает повязку* Теперь я понимаю, почему ты не жалуешься на боль.*(грустно)*

**Харви:** Приходи ко мне, когда вспомнишь что-то... Я буду слушать.*(грустно)*

---

<a id="eventharveyemergencycare-экстренная-помощь"></a>

### Экстренная помощь

*Hospital · `eventHarveyEmergencyCare`*

**Условия срабатывания**

- **Запуск:** Script-only (нет launcher)
  Ключ без preconditions. PlayEvent был в **отключённом** `triggersInjury.json`. C# выставляет buff/topic без cutscene.
- **CP preconditions:** *(нет — ключ `eventHarveyEmergencyCare` без `/Time`/`/GameStateQuery`)*

Всё плывёт перед глазами...

Ты чувствуешь, как кто-то подхватывает тебя на руки...

Ты приходишь в себя в клинике. Харви в белом халате с пятнами крови.

*Hospital*

**Харви:** ЧТО ТЫ НАДЕЛАЛА?!*(строго)*

**Харви:** Извини... Я не хотел кричать. Но ты чуть не погибла.*(грустно)*

**Харви:** Дай я осмотрю раны...*(грустно)*

---

<a id="eventharveyexhaustion-истощение"></a>

### Истощение

*Hospital · `eventHarveyExhaustion`*

**Условия срабатывания**

- **Запуск:** Script-only (нет launcher)
  Ключ без preconditions. BETAS-триггер в отключённом `triggersInjury.json`. C# — `topicFarmerExhausted` без Hospital-сцены.
- **CP preconditions:** *(нет — ключ `eventHarveyExhaustion` без `/Time`/`/GameStateQuery`)*

Твоё дыхание становится прерывистым, руки дрожат, а перед глазами пляшут чёрные точки...

Последнее, что ты видишь - как Харви резко бросает медицинские карты и бежит к тебе...

Ты приходишь в себя в клинике.

*Hospital*

**Харви:** Мару! Немедленно глюкозу 40% и кордиамин!*(серьёзно)*

**Maru:** Да, доктор!

**Харви:** *вводит инъекцию* Это поможет тебе прийти в себя... Дыши глубже. Вот так.*(строго)*

**Харви:** *голос дрожит от гнева* Ты... ты могла впасть в гипогликемическую кому!*(строго)*

**Харви:** Я нашел тебя без сознания... *стискивает зубы* Это больше не повторится.*(строго)*

**Харви:** *вытирает твой лоб влажной салфеткой* Всё хорошо... ты в безопасности...*(нежно)*
Тебе нужно отдохнуть. *ставит капельницу* Не бойся.* *(нежно)*

Ты слабо пытаешься протестовать...

**Харви:** *неожиданно мягко* Пожалуйста... *отходит на шаг, чтобы не давить* Дай мне позаботиться о тебе.
Спи сейчас. *приглушает свет* Я буду здесь, когда проснёшься.

---

<a id="eventharveytreatmentcollapse-коллапс-на-ферме"></a>

### Коллапс на ферме

*Hospital · `eventHarveyTreatmentCollapse`*

**Условия срабатывания**

- **Запуск:** Script-only (нет launcher)
  Ключ без preconditions. Нет C# `startEvent` / активного trigger.
- **CP preconditions:** *(нет — ключ `eventHarveyTreatmentCollapse` без `/Time`/`/GameStateQuery`)*

Голова кружится... Всё плывёт... Я не могу...

*Hospital*

Где я?.. Что со мной случилось?

**Харви:** Это был коллапс. Твоё тело просто отключилось от переутомления.*(грустно)*
С сегодняшнего дня ты на лечении. Под моим присмотром. Я очень прошу отнестись к этому серьёзно.*(строго)*
Я ввёл тебе Терацитин. Это поможет снять основное напряжение. Но будет немного побочных ощущений — ты можешь чувствовать усталость.

Харви… мне просто нужно немного отдохнуть…

**Харви:** Именно поэтому ты будешь отдыхать. Я буду проверять твоё состояние. Не вставай сначала покажись мне

---

<a id="eventstayinhospital-остаёшься-в-палате"></a>

### Остаёшься в палате

*Hospital · `eventStayInHospital`*

**Условия срабатывания**

- **Запуск:** Script-only (нет launcher)
  Ключ без preconditions. Госпитализацию делает C# `HospitalizationManager`, не это событие.
- **CP preconditions:** *(нет — ключ `eventStayInHospital` без `/Time`/`/GameStateQuery`)*

**Харви:** Куда ты собралась?*(строго)*
Я не говорил, что ты можешь вставать.*(строго)*

Мне нужно домой… Я уже лучше себя чувствую.

**Харви:** Ты ещё не готова. Я не хочу, чтобы ты снова рисковала здоровьем.*(строго)*
Ложись обратно. Сегодня лучше останься здесь ещё немного — как врач, я настаиваю.*(строго)*

---

## Часть IV. Шахта и раны (InjuryCare)

<a id="часть-iv-шахта-и-раны-injurycare"></a>

<a id="eventharveyminerescuedating-спасение-из-шахты-любовь"></a>

### Спасение из шахты (любовь)

*Mine · `eventHarveyMineRescueDating`*

**Условия срабатывания**

- **Запуск:** C# startEvent (+ дубль vanilla entry)
  `PassOutHandler.TriggerMineRescueEvents()` → утро после смерти в Mine → warp (17,7) → `startEvent`. CP-key с guards на seen/relationship при vanilla entry не проверяется при C#-запуске.
- **CP preconditions:**
  - Отношения с Harvey: Dating или Married; Не просмотрено событие `eventHarveyMineRescueDating`; Не просмотрено событие `eventHarveyMineRescue`; Не просмотрено событие `eventHarveyMinorMineRescue`
  - *Сырой ключ:* `eventHarveyMineRescueDating/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married, !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescueDating, !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescue, !PLAYER_HAS_SEEN_EVENT Current eventHarveyMinorMineRescue`
- **Дополнительно (C# InjuryCare):**
  - Вчера: HP ≤ 0 в локации Mine (боевая смерть)
  - Отношения с Харви: dating или married
  - Severe-травма (`buffBadlyHurt` и др.)
  - Событие ещё не в `eventsSeen` (иначе только topic `topicMineInjuryRescue`)

Темно… тяжело…

Шаги эхом отдаются в шахте…

**Харви:** фермер… Нет. Нет.*(в панике)*

**Харви:** Дыши. Смотри на меня.*(в панике)*

**Харви:** *проверяет пульс* Слабый… слишком слабый.*(в панике)*

**Харви:** Кровь… ладно. Сейчас главное — вынести тебя отсюда.*(строго)*
Я везу тебя в клинику. Пожалуйста, доверься мне.*(строго)*

**Харви:** Держись. Пожалуйста.*(грустно)*
Я рядом. Я не отпущу.*(грустно)*

*Hospital*

**Харви:** Ты в клинике. Не пытайся встать.*(серьёзно)*

**Харви:** Раны обработаны. Кровотечение остановлено.*(серьёзно)*
Обезболивающее подействует через несколько минут.*(серьёзно)*

**Харви:** Ты остаёшься под наблюдением.*(строго)*
Выйти из палаты сейчас нельзя. Я очень прошу отнестись к этому серьёзно.*(строго)*

Ты медленно открываешь глаза…

**Харви:** фермер! Ты в сознании.*(серьёзно)*
Не двигайся.*(серьёзно)*

**Харви:** Вчера ты потеряла сознание в шахте.*(грустно)*
Мне нужно завершить осмотр и начать лечение.*(серьёзно)*

**Харви:** *голос смягчается* Я буду рядом.*(нежно)*
Отдыхай. Я прослежу за каждым показателем.*(нежно)*

**Харви:** Если боль усилится — скажи сразу. Я не далеко.*(серьёзно)* *(конец сцены)*

---

<a id="eventharveyminerescue-спасение-из-шахты"></a>

### Спасение из шахты

*Mine · `eventHarveyMineRescue`*

**Условия срабатывания**

- **Запуск:** C# startEvent (fallback)
  Тот же C#-путь, если `eventHarveyMineRescueDating` отсутствует в `Data/Events/Mine`. При текущем CP почти не используется.
- **CP preconditions:**
  - Не просмотрено событие `eventHarveyMineRescue`; Не просмотрено событие `eventHarveyMineRescueDating`; Не просмотрено событие `eventHarveyMinorMineRescue`
  - *Сырой ключ:* `eventHarveyMineRescue/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescue, !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescueDating, !PLAYER_HAS_SEEN_EVENT Current eventHarveyMinorMineRescue`
- **Дополнительно (C# InjuryCare):**
  - Те же C#-условия, что у dating-версии
  - Fallback, если dating-entry нет в content pack

Ты теряешь сознание...

Быстрые шаги эхом отзываются в шахте...

**Харви:** фермер?! НЕТ!*(в панике)*

**Харви:** *проверяет пульс* Слабый... Критически слабый!*(в панике)*

**Харви:** Что ты здесь делаешь без сопровождения?!*(строго)*
Держись! Я вытащу тебя отсюда!*(строго)*

**Харви:** *осторожно поднимает на руки* Кожа холодная...*(грустно)*
Держись... пожалуйста, держись!*(грустно)*

*Hospital*

**Харви:** *срывающийся голос* Мару! Срочно реанимационный набор!*(строго)*

**Maru:** Доктор! Что случилось?!

**Харви:** Критическое состояние! Адреналин, физраствор, кислород!*(строго)*

**Харви:** *вводит инъекцию* Давай... стабилизируем пульс...*(грустно)*

**Харви:** *выдыхает* Пульс стабилизируется... Боже...*(грустно)*
Ещё минута — и могло быть хуже.*(грустно)*

Ты медленно открываешь глаза...

**Харви:** *замечает* фермер! Ты в сознании!*(с улыбкой)*
Не двигайся. Ты в клинике.*(серьёзно)*

**Харви:** Вчера ты потеряла сознание в шахте.*(грустно)*
У тебя серьёзные раны.*(строго)*
Мне нужно тебя осмотреть и начать лечение.*(серьёзно)*

**Харви:** Отдыхай сейчас.
Я буду следить за твоим состоянием.

**Харви:** Как боль? Нужно обезболивающее?*(грустно)* *(конец сцены)*

---

<a id="eventharveyminorminerescue-лёгкое-спасение-из-шахты"></a>

### Лёгкое спасение из шахты

*Mine · `eventHarveyMinorMineRescue`*

**Условия срабатывания**

- **Запуск:** C# startEvent
  C# выбирает minor при `!HasAnyBuff(Severe)`, но боевая смерть в шахте всегда даёт `buffBadlyHurt` → на практике **недостижимо**.
- **CP preconditions:**
  - Отношения с Harvey: Dating или Married
  - Не просмотрено событие `eventHarveyMinorMineRescue`
  - Не просмотрено событие `eventHarveyMineRescue`
  - Не просмотрено событие `eventHarveyMineRescueDating`
  - *Сырой ключ:* `eventHarveyMinorMineRescue/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyMinorMineRescue/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescue/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescueDating`
- **Дополнительно (C# InjuryCare):**
  - C#: `!HasAnyBuff(Severe)` после mine pass-out
  - Конфликт: mine combat death всегда даёт severe → minor не срабатывает

Голова кружится, дыхание сбивается... Ещё шаг — и ноги точно подведут.

**Харви:** фермер! Стой!*(в панике)*

**Харви:** Ты на грани. В таком состоянии я не дам тебе идти дальше.*(строго)*
Сейчас в клинику — покой и осмотр.*(серьёзно)*

*Hospital*

**Харви:** Садись. Дыши ровно. Я проверю показатели.*(серьёзно)*

**Харви:** Пульс и давление на нижней границе нормы...*(грустно)*
Ещё немного нагрузки — и могло стать хуже.*(серьёзно)*

**Харви:** Шахта подождёт. Сначала восстановление.
Если снова почувствуешь слабость — сразу ко мне, без героизма.*(серьёзно)*

**Харви:** Отдыхай. Я загляну позже.*(с улыбкой)* *(конец сцены)*

---

<a id="eventharveymineinterception-перехват-у-входа-в-шахту"></a>

### Перехват у входа в шахту

*Mine · `eventHarveyMineInterception`*

**Условия срабатывания**

- **Запуск:** SpaceCore PlayEvent
  `triggersCare.json` → `triggerHarveyMineWarning` при `LocationChanged` в Mine.
- **CP preconditions:** *(нет — ключ `eventHarveyMineInterception` без `/Time`/`/GameStateQuery`)*
- **SpaceCore trigger (`triggersCare.json`):**
  - Trigger `LocationChanged` → Отношения с Harvey: Dating или Married, Нет топика: `topicMineRescuePending`, Локация игрока: Mine, Активен бафф: `buffSurgicalWound` — хирургическая рана; `buffBruisedRibs` — ушиб рёбер; `buffSprainedAnkle` — растяжение; `buffBackStrain` — боль в спине; `buffDeepCuts` — глубокие порезы; `buffBurnWounds` — ожоги; `buffTornMuscles` — разрыв мышц; `buffConcussion` — сотрясение; `buffFracturedBone` — перелом; `buffShrapnelWounds` — осколочные раны; `buffInfectedWound` — инфекция

**Харви:** Стой.*(строго)*
Стой прямо сейчас.*(строго)*

**Харви:** Я же говорил тебе - никаких шахт во время лечения!*(строго)*
Ты думаешь, это игра?*(строго)*

Я... я просто хотела...

**Харви:** Хотела что? Усугубить стресс? Получить еще раны?*(строго)*
*осматривает тебя* Посмотри на себя - ты вся дрожишь!*(серьёзно)*

**Харви:** *снимает с себя куртку и накидывает на фермера*
Давай вернёмся домой — тебе нужен покой.*(строго)*

Но мне нужна руда для...

**Харви:** Руда?*(в панике)* РУДА?!
Единственное, что тебе сейчас нужно - это покой!*(строго)*

**Харви:** *осторожно берёт за запястья — проверяет пульс*
Твои ладони ледяные...
Пульс учащённый...
Дыхание поверхностное...*(грустно)*

**Харви:** фермер, ты на грани нервного срыва.*(грустно)*
И ты хочешь спуститься в тёмную, опасную шахту?*(грустно)*

**Харви:** Я не хочу, чтобы ты снова навредила себе.*(строго)*
Не тогда, когда ты под моей опекой.*(строго)*

Я не маленький ребёнок...

**Харви:** Нет, ты не ребёнок.
Ты - моя пациентка.
И пока ты не выздоровеешь полностью, я предложу план лечения, но окончательное слово — за тобой.*(строго)*

**Харви:** *достаёт из сумки термос*
Вот успокаивающий чай.
Пей здесь.

**Харви:** Знаешь, что меня больше всего пугает?*(грустно)*
Не то, что ты можешь пострадать в шахте...*(грустно)*

**Харви:** А то, что ты бежишь туда, спасаясь от собственных мыслей.*(грустно)*
Но от стресса не убегают вглубь земли.
От него лечатся.*(серьёзно)*

**Харви:** *поправляет куртку на твоих плечах*
Завтра утром - полное обследование.
И никаких шахт до моего разрешения.*(строго)*

А если я не послушаюсь?

**Харви:** *улыбается, но в глазах стальная решимость*
Тогда я буду следовать за тобой везде.*(строго)*
В шахту, в лес, в пустыню...
Пока ты не поймёшь, что твоё здоровье важнее любых сокровищ.*(строго)*

**Харви:** *встаёт и протягивает руку*
А теперь пойдём домой.
Я приготовлю тебе ужин и проверю давление.

Харви ведёт тебя прочь от шахт, поддерживая за локоть.

**Харви:** И запомни раз и навсегда: Пока я твой врач - моё слово закон.*(строго)*
А я планирую быть твоим врачом очень, очень долго.*(нежно)*

---

<a id="eventharveyskullcaveprevention-пещера-черепов"></a>

### Пещера черепов

*SkullCave · `eventHarveySkullCavePrevention`*

**Условия срабатывания**

- **Запуск:** SpaceCore PlayEvent
  `triggerLocationReactionSkullCaveExit` (SkullCave) и `triggerHarveySkullCaveWarning` (Mine+SkullCave — условие битое).
- **CP preconditions:** *(нет — ключ `eventHarveySkullCavePrevention` без `/Time`/`/GameStateQuery`)*
- **SpaceCore trigger (`triggersCare.json`):**
  - Trigger `LocationChanged` → Отношения с Harvey: Dating или Married, Нет топика: `topicMineRescuePending`, Локация игрока: SkullCave, Активен бафф: `buffSurgicalWound` — хирургическая рана; `buffBruisedRibs` — ушиб рёбер; `buffSprainedAnkle` — растяжение; `buffBackStrain` — боль в спине; `buffDeepCuts` — глубокие порезы; `buffBurnWounds` — ожоги; `buffTornMuscles` — разрыв мышц; `buffConcussion` — сотрясение; `buffFracturedBone` — перелом; `buffShrapnelWounds` — осколочные раны; `buffInfectedWound` — инфекция
  - Trigger `LocationChanged` → Локация игрока: SkullCave, Отношения с Harvey: Dating или Engaged или Married, Нет топика: `topicMineRescuePending`

**Харви:** фермер! Немедленно выйди отсюда!*(строго)*
Пещера черепа — не место для игр! Здесь можно умереть!*(строго)*

**Харви:** Ты понимаешь, что тут происходит?*(строго)*
Монстры, которые могут убить за секунду! Яд, проклятия, смертельные ловушки!?*(строго)*

**Харви:** А ты приходишь сюда одна! Без защиты! Без подготовки!*(строго)*
Без... без меня.*(грустно)*

**Харви:** Если что-то случится с тобой здесь...*(грустно)*
Если я потеряю тебя в этом проклятом месте... Я себе этого не прощу.*(грустно)*

**Харви:** Домой. Сейчас же! И больше сюда не возвращайся без меня. Обещай мне это!*(строго)*

**Выбор:**
- Обещаю
- Но мне нужны ресурсы...

**Харви:** Спасибо. Теперь я буду спокоен. Пойдём домой.*(нежно)*

---

## Часть V. Забота и ночные тревоги

<a id="часть-v-забота-и-ночные-тревоги"></a>

<a id="eventharveycheckhealthfarmer-проверка-после-обморока"></a>

### Проверка после обморока

*Farm · `eventHarveyCheckHealthFarmer`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Farm` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 06:00–12:00
  - Просмотрено событие `PlayerKilled`
  - Отношения с Harvey: Dating
  - *Сырой ключ:* `eventHarveyCheckHealthFarmer/Time 600 1200/GameStateQuery PLAYER_HAS_SEEN_EVENT Current PlayerKilled/GameStateQuery PLAYER_NPC_RELATIONSHIP  Current Harvey Dating`

**Харви:** Ты выглядишь ужасно!*(в панике)*
*быстро подходит* Давай проверим...*(серьёзно)*

**Харви:** *берёт за запястье* Пульс 110, зрачки расширены... *ощупывает лоб* И температура повышена!*(серьёзно)*

**Харви:** Немедленно в клинику! Я не спрашиваю — я приказываю.*(строго)*

Но я просто устала...

**Харви:** Нет! *сжимает её плечи* Ты даже дрожишь.*(строго)*

**Харви:** *крепко берёт под руку* Ты еле стоишь. Вот опора.*(строго)*
Шаг за шагом... *поддерживает* Я буду рядом на каждом шагу.*(строго)*

*Hospital*

**Харви:** *укладывает на койку* Вот так... *поправляет подушку* Теперь ты никуда не денешься.*(строго)*

Это перебор...

**Харви:** Перебор - это твоё состояние! *ставит капельницу* Я отменил все приёмы - сегодня ты мой единственный пациент.*(строго)*

Харви привязывает её к койке (шутливо), ставит капельницу и запрещает вставать.

**Харви:** Если увижу, что ты взяла кирку — вызову санитаров!*(строго)* *(конец сцены)*

---

<a id="eventharveycheckfarmeroutsideafter22-ночная-прогулка"></a>

### Ночная прогулка

*Farm · `eventHarveyCheckFarmerOutsideAfter22`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Farm` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 22:00–02:00
  - Активен разговорный топик: `topicPassedOutInTown` (обморок в городе (C# PassOutHandler))
  - Отношения с Harvey: Dating или Married
  - *Сырой ключ:* `eventHarveyCheckFarmerOutsideAfter22/Time 2200 0200/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicPassedOutInTown/GameStateQuery PLAYER_NPC_RELATIONSHIP  Current Harvey Dating Married`

**Харви:** фермер?!*(строго)*

**Харви:** Что ты делаешь на улице в это время?*(строго)*
Сейчас же марш в дом!*(строго)*

**Выбор:**
- Я просто...
- Хотела проверить животных.
- Не могу спать(break)speak Harvey "Животные подождут до утра!*(строго)*"(break)speak Harvey "Это не повод гулять ночью!*(строго)*

**Харви:** Записываю: сегодня - нарушение режима. Завтра дополнительный осмотр в 8:00.*(строго)*
И если я ещё раз увижу тебя ночью на улице...*(строго)*
Я привяжу тебя к кровати.*(строго)*

---

<a id="eventharveymorningcheckup-утренний-осмотр"></a>

### Утренний осмотр

*Farm · `eventHarveyMorningCheckup`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Farm` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 06:00–08:00
  - Активен разговорный топик: `topicHarveyMandatoryCheckup` (после ночной проверки / обморока)
  - Отношения с Harvey: Dating
  - *Сырой ключ:* `eventHarveyMorningCheckup/Time 0600 0800/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveyMandatoryCheckup/GameStateQuery PLAYER_NPC_RELATIONSHIP  Current Harvey Dating`

**Харви:** фермер? Проснись, солнышко...*(нежно)*

**Харви:** Я принёс завтрак в постель.*(с улыбкой)*
И витаминный чай для восстановления.*(нежно)*

**Харви:** Давай проверим твоё состояние...*(серьёзно)*
Пульс нормализовался, но организм всё ещё истощён.*(грустно)*
Вчерашний инцидент не должен повториться.*(строго)*

**Выбор:**
- Спасибо за заботу...
- Я просто не могла уснуть.
- Ты слишком волнуешься.(break)speak Harvey "*(грустно)* Я понимаю... Но мы должны найти причину. Может быть, тревожные мысли? Боль? Я могу приготовить успокаивающий чай."addItem 614 1(break)speak Harvey "Это моя работа - волноваться. И моя привилегия - заботиться.*(нежно)*

**Харви:** Вот щадящий режим на сегодня:
1. Лёгкая работа до 14:00
2. Обязательный отдых после обеда
3. Травяная ванна перед сном

---

<a id="eventharveylatenightcollapse-обморок-в-городе"></a>

### Обморок в городе

*Town · `eventHarveyLateNightCollapse`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Town` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 00:00 (след. день)–02:00 (след. день)
  - *Сырой ключ:* `eventHarveyLateNightCollapse/Time 2400 2600`

**Харви:** фермер?! Чёрт возьми!*(строго)*

**Харви:** Пульс... дыхание...*(серьёзно)*
Просто истощение.*(грустно)*

**Харви:** Это уже третий случай на этой неделе!*(строго)*
Я везу тебя в клинику.*(строго)*

**Харви:** Нет, я очень прошу отнестись к этому серьёзно. Ты идешь на капельницу.*(строго)*

*Hospital*

**Харви:** *(строго)* Вот твой режим на неделю:
1. Отбой в 22:00 (я буду проверять)
2. Обязательный дневной сон
3. Эти тонизирующие капсулы

**Харви:** И... Если снова почувствуешь слабость - звони.
Я приду за 60 секунд. Проверено.

**Харви:** Спокойной ночи... хотя сейчас уже утро.*(грустно)* *(конец сцены)*

---

## Часть VI. Гроза и страх

<a id="часть-vi-гроза-и-страх"></a>

<a id="eventrescueoperation-операция-спасения"></a>

### Операция спасения

*Woods · `eventRescueOperation`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Woods` при выполнении условий ниже
- **CP preconditions:**
  - Погода: гроза
  - Активен разговорный топик: `topicRescueOperation` (запуск операции спасения)
  - Не просмотрено событие `eventRescueOperation`
  - *Сырой ключ:* `eventRescueOperation/Weather Storm/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicRescueOperation/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventRescueOperation`

*Hospital*

**Харви:** Алло? Льюис?

**Lewis:** Харви! Я сбил фермер! Она выбежала на дорогу во время грозы!*(в панике)*

**Харви:** ЧТО?! Где она сейчас?!*(в панике)*

**Lewis:** Не знаю! Она убежала в лес! У неё кровь на виске!*(в панике)*

**Харви:** *хватает сумку* Встречаемся у леса. НЕМЕДЛЕННО!*(строго)*

*Woods*

*ползёт к густым кустам*

*кровь течёт по лицу*

*сжимается в комок под кустом*

**Харви:** Где ты её видел в последний раз?!*(в панике)*

**Lewis:** Здесь, рядом с этими кустами! Боже, сколько крови...*(грустно)*

**Харви:** фермер! Откликнись!*(строго)*

**Харви:** Нашёл! Льюис, она здесь!*(в панике)*

**Lewis:** Слава богу... Жива?*(грустно)*

**Харви:** фермер, это я, Харви. Мне нужно осмотреть твою голову.*(грустно)*

*отползает глубже* Опасно...

**Харви:** Что опасно? Я врач, я помогу.*(грустно)*

*качает головой* Спрячься... они услышат...

**Lewis:** О чём она говорит?*(грустно)*

**Харви:** Льюис, отойди. Дай мне поговорить с ней.*(строго)*

**Харви:** *тихо* фермер, посмотри на меня. Кто услышит?*(грустно)*

*прижимается к земле* Прячься!...

**Харви:** *понимающе* Не бойся... Это гром.*(грустно)*
*медленно протягивает руку* Возьми, если готова. Если нет — я подожду.

*боится прикосновений* Нет... не трогай...

**Харви:** Я не сделаю тебе больно. Обещаю.
Но мне нужно вытащить тебя отсюда.*(строго)*

*шёпотом* Ты спрячешься?

**Харви:** Да, мы спрячемся. В клинике. Там безопасно.

**Харви:** *осторожно поднимает — только если ты не отстраняешься* Опирайся на меня.

*хромает, держится за бок*

**Харви:** Рёбра? Льюис, что именно произошло?!*(строго)*

**Lewis:** Я не видел её из-за дождя... она выбежала прямо под колёса...*(грустно)*

**Харви:** *проверяет пульс* Пульс учащён, возможно сотрясение.*(серьёзно)*
Льюис, ведите к пикапу. Быстро!*(строго)*

*Forest*

**Lewis:** *открывает дверь* Осторожно...*(грустно)*

**Харви:** *помогает сесть* Держись. Едем в клинику.

*молча кивает, дрожит*

*Hospital*

**Харви:** Льюис, можете идти. Спасибо за помощь.*(грустно)*

**Lewis:** Если что-то понадобится... Я буду дома.*(грустно)*

**Харви:** *закрывает дверь* Теперь никого лишних. Безопасно.

**Харви:** Вот сухая одежда. Переоденься, а я осмотрю раны.*(строго)*

*не двигается, смотрит в пол*

**Харви:** фермер... *мягче* Я повернусь спиной. Скажи, когда будешь готова.*(грустно)*

**Харви:** Рана на голове неглубокая, но нужно обработать.*(серьёзно)*

*тихо* Готово...

**Харви:** Теперь покажи, где болят рёбра.*(серьёзно)*

*медленно приподнимает рубашку*

**Харви:** Обширная гематома... *пальпирует* Есть болезненность, но перелома нет.*(серьёзно)*

**Харви:** Повернись. Нужно проверить, нет ли травм спины.*(строго)*

*замирает, боится*

**Харви:** Что-то не так? Там тоже болит?*(грустно)*

*очень медленно поворачивается спиной*

**Харви:** *резко втягивает воздух* Что... Боже мой...*(в панике)*

*быстро поворачивается, опускает рубашку*

**Харви:** фермер... *дрожащим голосом* Эти шрамы... Кто это сделал?*(грустно)*

*молчит, обнимает себя руками*

**Харви:** *подходит ближе* Сколько раз ломали рёбра? Сколько раз били?*(грустно)*

*качает головой, не может говорить*

**Харви:** *медленно протягивает руки — ждёт, пока ты сама не ответишь* Всё позади. Понимаешь? Всё позади.

**Харви:** *голос становится жёстким* Тот, кто это сделал... если он когда-нибудь появится здесь...*(строго)*

*прижимается к нему*

**Харви:** *успокаивается* Прости. Не хотел пугать.*(грустно)*
*кладёт ладонь на плечо — ненадолго* Ты в безопасности.

**Харви:** Сейчас обработаю рану на голове, и я предложу тебе остаться под моим присмотром.*(строго)*
Никаких больниц, никаких чужих людей. Только я буду дежурить.

**Выбор:**
- *кивает и не отпускает его*
- *шёпотом* Не оставляй меня одну...
- *молча прижимается ближе*(break)friendship Harvey 220speak Harvey "*обнимает — только потому что ты не отстраняешься* Я никуда не уйду.
- Сегодня, завтра... сколько понадобится."emote Harvey 20(break)friendship Harvey 200speak Harvey "*голос дрожит* Никогда не оставлю.
- *опускает лоб к твоему — коротко, почти не касаясь* Ты под моей защитой."(break)friendship Harvey 240speak Harvey "*сжимает ладонь — если ты сама не отпрятала руку* Я буду рядом каждую минуту.
- *решительно* Обещаю.*(строго)*

---

## Часть VII. Сердце и свидания

<a id="часть-vii-сердце-и-свидания"></a>

<a id="eventharveyfirstdate-первое-свидание"></a>

### Первое свидание

*Forest · `eventHarveyFirstDate`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Forest` при выполнении условий ниже
- **CP preconditions:**
  - Погода: солнечно
  - Дружба с Harvey ≥ 8 сердечек (2000 pts)
  - Время суток: 18:00–02:00 (след. день)
  - Отношения с Harvey: Dating
  - SEASON Spring Summer Fall
  - *Сырой ключ:* `eventHarveyFirstDate/Weather Sunny/Friendship Harvey 2000/Time 1800 2600/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating/GameStateQuery SEASON Spring Summer Fall`

**Харви:** Ты пришла! *нервно поправляет галстук* Я... э-э... подготовил всё для твоего комфорта.*(нежно)*

**Харви:** Плед с подогревом, термос с имбирным чаем... *раскладывает подушки* И бинт на случай, если подвернешь ногу.*(нежно)*

Ты даже градусник принес?

**Харви:** *краснеет* Это... профессиональная деформация.

**Харви:** Смотри... *укутывает тебя пледом* Как раз начинается закат.*(нежно)*

**Харви:** Только не замёрзни. Вот термос... и моя рука для тепла. *осторожно берёт твою ладонь* *(нежно)*

**Харви:** Знаешь... когда ты упала в тот день... *голос дрожит* Я впервые за 10 лет практики испугался по-настоящему.*(грустно)*

Но ты же меня спас...

**Харви:** Я не хочу быть просто твоим врачом. *крепче сжимает руку* *(нежно)*
Позволь мне стать твоим защитником, опорой... может быть, чем-то большим?*(нежно)*

**Харви:** *достаёт коробочку* Это не кольцо... пока. Просто медицинский браслет с моим номером. На случай... ну ты понимаешь.*(нежно)*

Я...

**Харви:** Не отвечай сейчас. *поправляет плед* Просто знай - я всегда рядом.*(нежно)*

---

<a id="eventharveymountaindate-свидание-в-горах"></a>

### Свидание в горах

*Mountain · `eventHarveyMountainDate`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Mountain` при выполнении условий ниже
- **CP preconditions:**
  - Погода: солнечно
  - Время суток: 09:00–12:00
  - Дружба с Harvey ≥ 9 сердечек (2250 pts)
  - Отношения с Harvey: Dating
  - *Сырой ключ:* `eventHarveyMountainDate/Weather sunny/Time 900 1200/Friendship Harvey 2250/GameStateQuery PLAYER_NPC_RELATIONSHIP  Current Harvey Dating`

**Харви:** *(с улыбкой)* фермер... я подумал, что тебе понравится это место. Здесь так тихо... и спокойно.

Здесь действительно красиво. Ты прав.

**Харви:** Я часто мечтал привести тебя сюда. Не как врач... а как человек, который хочет быть рядом.

**Харви:** Ты знаешь, почему я выбрал именно это место?

Почему?

**Харви:** Потому что здесь ты можешь просто дышать... без забот, без работы... только мы двое.

**Харви:** Спасибо, что ты есть. Я бы не хотел проводить эти моменты ни с кем другим.

---

<a id="eventharveypropose-предложение"></a>

### Предложение

*Beach · `eventHarveyPropose`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Beach` при выполнении условий ниже
- **CP preconditions:**
  - Погода: солнечно
  - Дружба с Harvey ≥ 10 сердечек (2500 pts)
  - Время суток: 18:00–02:00 (след. день)
  - Отношения с Harvey: Dating
  - SEASON Spring Summer Fall
  - *Сырой ключ:* `eventHarveyPropose/Weather Sunny/Friendship Harvey 2500/Time 1800 2600/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating/GameStateQuery SEASON Spring Summer Fall`

**Харви:** Пришел пораньше... *стоит у воды в расстегнутой рубашке* Чтобы проверить - нет ли медуз у берега.*(нежно)*

**Харви:** Выбрал самое безопасное место. Песок мягкий, ветер слабый, течение спокойное.*(нежно)*

Ты даже море проверил?

**Харви:** Конечно. *уверенно* Ты же будешь здесь купаться.

**Харви:** Подожди. *накидывает на тебя лёгкое одеяло* Вечерний бриз может быть коварным.*(нежно)*

Но ведь жарко...

**Харви:** Именно поэтому. *поправляет шаль* Ты простудишься, если вспотевшая выйдешь на ветер.*(нежно)*

**Харви:** Вода идеальная. *берёт тебя за руку* Не бойся, я буду рядом каждую секунду.*(нежно)*

**Харви:** Держись за меня... Вот так. *крепко обнимает* Я не дам ни одной волне коснуться тебя.*(нежно)*

**Харви:** Знаешь, раньше я измерял счастье в показателях анализов... *гладит по мокрым волосам* Но ты научила меня другому.*(нежно)*

**Харви:** Я не спросил тогда, в горах... *достаёт кольцо из ракушки* Потому что хотел сделать это здесь.

**Харви:** Позволь мне быть твоим берегом, твоей опорой, твоим... *голос дрожит* ...мужем?*(нежно)*

...

**Харви:** Не отвечай сейчас, если не ${готов^готова}$. Я просто хотел, чтобы ты ${знал^знала}$: я выбираю тебя. Но решение — только твоё.*(нежно)*

---

<a id="eventharveyroomcheckup-осмотр-в-комнате"></a>

### Осмотр в комнате

*HarveyRoom · `eventHarveyRoomCheckup`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `HarveyRoom` при выполнении условий ниже
- **CP preconditions:**
  - Дружба с Harvey ≥ 6 сердечек (1500 pts)
  - *Сырой ключ:* `eventHarveyRoomCheckup/Friendship Harvey 1500`

**Харви:** Плановый медицинский осмотр!*(серьёзно)*
фермер, я заметил, что ты чихала. Это плановый осмотр.

Ты пытаешься сбежать, но Харви останавливает тебя за локоть — мягко, но настойчиво.

**Харви:** Так... Глубокий вдох.*(строго)*

Я здорова!

**Харви:** Ты всегда так говоришь. Вот термометр.*(серьёзно)*

---

<a id="eventharveyroomcheckup2-неожиданный-визит"></a>

### Неожиданный визит

*HarveyRoom · `eventHarveyRoomCheckup2`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `HarveyRoom` при выполнении условий ниже
- **CP preconditions:**
  - Отношения с Harvey: Dating
  - Spiderbuttons.BETAS_NPC_LOCATION Harvey HarveyRoom
  - Случайность 20%
  - *Сырой ключ:* `eventHarveyRoomCheckup2/GameStateQuery PLAYER_NPC_RELATIONSHIP  Current Harvey Dating/GameStateQuery Spiderbuttons.BETAS_NPC_LOCATION Harvey HarveyRoom/GameStateQuery RANDOM 0.2`

**Харви:** Плановый осмотр, фермер! *достаёт стетоскоп* Я заметил ты чихала утром - это тревожно.*(строго)*

Ты пятишься к двери...

**Харви:** О нет, ты не... *быстро блокирует дверь* *(строго)*

Ты бросаешься к окну!

**Харви:** Стой! *перехватывает за талию* Это опасно!*(в панике)*

Я ненавижу осмотры! *голос дрожит*

**Харви:** *мягко* Я знаю... *приседает на уровень глаз* Но я не причиню тебе боли.*(нежно)*

**Харви:** Давай договоримся - только пульс и давление. *достаёт тонометр* И чай с мёдом после.*(нежно)*

**Харви:** Вот видишь, не так страшно?*(нежно)*
Пульс 90... от страха, не от болезни.*(нежно)*

**Харви:** Теперь о наказании за побег... *достаёт витамины* Двойная доза сегодня.*(строго)*

---

## Приложение: прочие события

<a id="appendix"></a>

### HarveyOverhaulStory.E2B_QuietAgreement

*Town · `HarveyOverhaulStory.E2B_QuietAgreement`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Town` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 10:00–16:00
  - Погода: солнечно
  - Не день фестиваля
  - Дружба с Harvey ≥ 3 сердечек (750 pts)
  - Просмотрено событие `HarveyOverhaulStory.E2_InsistentExam`
  - Не просмотрено событие `HarveyOverhaulStory.E2B_QuietAgreement`
  - Нет топика: HarveyMod_CD_Global
  - Нет топика: HarveyMod_CD_E2
  - *Сырой ключ:* `HarveyOverhaulStory.E2B_QuietAgreement/Time 1000 1600/Weather Sunny/!FestivalDay/Friendship Harvey 750/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E2_InsistentExam/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E2B_QuietAgreement/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E2`

Ты останавливаешься у лавки, прислонившись к стволу дерева.

Харви уже идёт быстрым шагом — и замирает в двух шагах от вас.

**Харви:** Я вижу, что вы устали. И очень хочу прямо сейчас начать врачебную лекцию.*(строго)*

**Харви:** Но на площади это будет не помощь, а спектакль. Поэтому коротко: вода, тень или клиника?

**Выбор:**
- Взять воду
- Отойти в тень
- Пойти в клинику
- Покачать головой(break)speak Harvey "Хорошо. Без лекции. Только вода."message "Он протягивает бутылку так, будто это самый обычный жест в мире."speak Harvey "Медленно. Я всё равно считаю глотки, но делаю вид, что нет."(break)speak Harvey "Тень — разумно."move farmer -1 0 3move Harvey -1 0 3speak Harvey "Видите? Я могу быть доволен пациентом почти молча.*(нежно)*"(break)speak Harvey "Хороший выбор."speak Harvey "И нет, это не поражение. Это технически грамотное отступление."move Harvey -3 0 3move farmer -3 0 3message "Он идёт рядом — без спешки, направление одно: к клинике."(break)speak Harvey "Понял. Не давлю."speak Harvey "Но я оставлю бутылку на лавке. Совершенно случайно. Как городской пейзаж."pause 300message "Бутылка остаётся на краю лавки.

**Харви:** Договоримся так: я не устраиваю осмотр на людях, а вы не делаете вид, что тело — необязательная часть фермерства.*(строго)*

**Харви:** Кивок подойдёт.

**Выбор:**
- Кивнуть
- Покачать головой
- Отвернуть взгляд(break)emote farmer 40speak Harvey "Хорошо. Договор зафиксирован."speak Harvey "Кивка достаточно.*(нежно)*"(break)emote farmer 12speak Harvey "Понял. Настаивать не буду."speak Harvey "Но я всё равно здесь.*(строго)*"(break)message "Ты отводишь взгляд к лавке."speak Harvey "Не хотите смотреть в глаза — уважаю."speak Harvey "Если я ошибся — покажите.

---

### HarveyOverhaulStory.E3B_WingPatient

*Forest · `HarveyOverhaulStory.E3B_WingPatient`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Forest` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 12:00–18:00
  - Погода: солнечно
  - Не день фестиваля
  - Дружба с Harvey ≥ 4 сердечек (1000 pts)
  - Просмотрено событие `HarveyOverhaulStory.E3_ForestApothecary`
  - Не просмотрено событие `HarveyOverhaulStory.E3B_WingPatient`
  - Нет топика: HarveyMod_CD_Global
  - Нет топика: HarveyMod_CD_E3
  - *Сырой ключ:* `HarveyOverhaulStory.E3B_WingPatient/Time 1200 1800/Weather Sunny/!FestivalDay/Friendship Harvey 1000/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E3_ForestApothecary/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E3B_WingPatient/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E3`

В траве у тропы что-то тихо шевелится.

**Харви:** Стойте. Не наступайте.*(в панике)*

Ты замираешь на полушаге.

**Харви:** Птица. Крыло повреждено.*(серьёзно)*

**Харви:** Мне нужны ваши руки. Спокойные. Осторожные.

**Выбор:**
- Подать платок
- Придержать корзину
- Отойти, чтобы не мешать
- Показать на мягкий мох(break)message "Ты протягиваешь сложенный платок — медленно, обеими руками."speak Harvey "Спасибо. Чистый? Хорошо."speak Harvey "Вот так. Маленькая повязка для очень возмущённого пациента.*(серьёзно)*"(break)message "Ты придерживаешь корзину на коленях — ровно, без дрожи."speak Harvey "Держите ровно. Да, именно так."speak Harvey "У вас хорошие руки для таких вещей. Тихие.*(нежно)*"(break)message "Ты отступаешь на два шага и замираешь."move farmer 0 2 2speak Harvey "Правильно. Если страшно — лучше отойти, чем дёрнуться в последний момент."speak Harvey "Вы всё равно помогаете. Пространство — тоже помощь.*(нежно)*"(break)message "Ты указываешь на мягкий мох у корней — короткий жест, без слов."speak Harvey "Мягко и влажно… да, подойдёт."speak Harvey "Вы снова нашли решение раньше меня. Начинаю подозревать, что лес консультирует вас напрямую.*(с улыбкой)*

**Харви:** Я отнесу его в клинику. Ненадолго. Не смотрите так — у меня были пациенты куда менее благодарные и куда более кусачие.

**Харви:** Вы хорошо справились. Не с птицей — с тревогой.*(нежно)*

Харви закрывает корзину платком, но оставляет щель для воздуха.

---

### HarveyOverhaulStory.E4B_TooQuiet

*Mountain · `HarveyOverhaulStory.E4B_TooQuiet`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Mountain` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 18:00–22:00
  - Погода: солнечно
  - Не день фестиваля
  - Дружба с Harvey ≥ 6 сердечек (1500 pts)
  - Просмотрено событие `HarveyOverhaulStory.E4_PierBreath`
  - Не просмотрено событие `HarveyOverhaulStory.E4B_TooQuiet`
  - Нет топика: HarveyMod_CD_Global
  - Нет топика: HarveyMod_CD_E4
  - *Сырой ключ:* `HarveyOverhaulStory.E4B_TooQuiet/Time 1800 2200/Weather Sunny/!FestivalDay/Friendship Harvey 1500/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E4_PierBreath/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E4B_TooQuiet/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E4`

Харви стоит у перил и смотрит вниз, на огни города.

**Харви:** Вы тоже заметили?

**Выбор:**
- Кивнуть
- Покачать головой
- Показать на город(break)emote farmer 40speak Harvey "Хорошо. Кивка достаточно."(break)emote farmer 12speak Harvey "Понял. Не хотите говорить — не буду."speak Harvey "Если я ошибся — покажите."(break)message "Ты поворачиваешься к огням долины."speak Harvey "Да. Там другой шум. Тоже настоящий.

**Харви:** Здесь слишком тихо. После клиники тишина иногда звучит громче, чем люди.*(грустно)*

**Харви:** Не волнуйтесь. Я в порядке. Это врачебное «в порядке», то есть почти правда.

**Выбор:**
- Остаться рядом
- Протянуть чай
- Сделать шаг назад
- Посмотреть на город(break)message "Ты остаёшься на месте — не ближе, но и не уходишь."speak Harvey "Спасибо."speak Harvey "Я обычно сам говорю это пациентам: не обязательно говорить. Просто не уходите резко."speak Harvey "Забавно, когда собственные советы догоняют сзади.*(с улыбкой)*"(break)message "Ты протягиваешь термос с чаем — молча, обеими руками."speak Harvey "Это мне?"speak Harvey "Вот теперь я официально пережил профессиональное поражение. Пациент принёс врачу чай.*(с улыбкой)*"speak Harvey "И, кажется, это помогает.*(нежно)*"(break)message "Ты делаешь шаг назад — оставляя ему воздух у перил."speak Harvey "Хорошо. Пространство."speak Harvey "Вы учите меня этому лучше любых учебников.*(нежно)*"(break)message "Ты поворачиваешься к огням города — так же, как он."speak Harvey "Да. Там все спят, спорят, забывают лекарства, падают с лестниц, едят сомнительные грибы…"speak Harvey "И почему-то я всё равно люблю эту работу.*(нежно)*

**Харви:** Не пугайтесь. Я не перестану быть настойчивым.*(строго)*

**Харви:** Просто иногда мне полезно помнить: рядом — это не только когда я держу кого-то за локоть.

**Харви:** Иногда рядом — это когда кто-то молчит у перил.*(нежно)*

---

### HarveyOverhaulStory.E9_LightInWindow

*Town · `HarveyOverhaulStory.E9_LightInWindow`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Town` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 20:00–23:30
  - Не день фестиваля
  - Дружба с Harvey ≥ 9 сердечек (2250 pts)
  - Просмотрено событие `HarveyOverhaulStory.E8_QuietShelf`
  - Не просмотрено событие `HarveyOverhaulStory.E9_LightInWindow`
  - Нет топика: HarveyMod_CD_Global
  - Нет топика: HarveyMod_CD_E8
  - *Сырой ключ:* `HarveyOverhaulStory.E9_LightInWindow/Time 2000 2330/!FestivalDay/Friendship Harvey 2250/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E8_QuietShelf/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E9_LightInWindow/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E8`

В окне клиники всё ещё горит свет.

На подоконнике стоит кружка, рядом — раскрытый журнал приёма.

**Выбор:**
- Остановиться у окна
- Постучать
- Пройти мимо
- Оставить травяной пучок у двери(break)message "Ты останавливаешься у окна."emote farmer 56pause 800playSound doorClosewarp Harvey 37 88faceDirection Harvey 0pause 300faceDirection Harvey 3speak Harvey "Я видел вас из окна."speak Harvey "Да, я всё ещё работаю. Нет, это не пример для подражания.*(строго)*"speak Harvey "Но если вы стоите здесь, значит, либо вам плохо, либо вы тоже не умеете вовремя уходить домой."(break)message "Ты стучишь в дверь клиники."playSound woodySteppause 400playSound doorClosewarp Harvey 37 88faceDirection Harvey 0speak Harvey "Входите."speak Harvey "Я сказал это слишком быстро, да?*(серьёзно)*"speak Harvey "Профессиональная привычка. Когда кто-то стучит в клинику вечером, я уже мысленно ищу бинты."(break)message "Ты делаешь шаг вперёд — будто собираешься пройти мимо."move farmer 1 0 1pause 600playSound doorClosewarp Harvey 34 88faceDirection Harvey 1move farmer -1 0 3faceDirection farmer 1speak Harvey "Я не буду задерживать."speak Harvey "Только скажу: дорога к ферме темнее обычного. Фонарь возьмите.*(строго)*"speak Harvey "Да, это была забота. Очень сдержанная, почти незаметная. Я расту.*(с улыбкой)*"(break)message "Ты оставляешь связку трав у порога."pause 500playSound doorClosewarp Harvey 37 88faceDirection Harvey 0emote Harvey 8speak Harvey "Это мне?"speak Harvey "Для чая?"speak Harvey "Вы молчите так выразительно, что мне начинает казаться, будто я понимаю лесной этикет.*(с улыбкой)*

Харви смотрит на вас — коротко, по-врачебному, без осмотра.

**Харви:** Ладно. Состояние терпимое.

**Харви:** Что вам сейчас нужно?

**Выбор:**
- Чай
- Пять минут тишины
- Проводить домой
- Только фонарь(break)message "Ты берёшь чашку обеими руками и киваешь."speak Harvey "Кивок засчитан. И, кажется, чай тоже.*(с улыбкой)*"speak Harvey "Без осмотра, если только вы не начнёте падать со стула. Тогда извините, включится врач.*(строго)*"friendship Harvey 45(break)speak Harvey "Пять минут тишины."speak Harvey "Я поставлю таймер. Шучу. Почти.*(с улыбкой)*"message "Ты садишься на ступеньках клиники — Харви молчит, не нависая."friendship Harvey 50(break)speak Harvey "Конечно."speak Harvey "Я хотел предложить сам, но очень старался быть воспитанным.*(с улыбкой)*"friendship Harvey 48(break)speak Harvey "Разумно."speak Harvey "Фонарь, и я смотрю с крыльца, пока вы не свернёте к ферме."speak Harvey "Это компромисс. Медицински сомнительный, но эмоционально приемлемый.*(с улыбкой)*"message "Он протягивает карманный фонарь — тяжёлый, надёжный, явно запасной."friendship Harvey 42

**Харви:** Я не обещаю перестать волноваться.*(серьёзно)*

**Харви:** Не смогу. Не с вами.

**Харви:** Но я могу обещать другое: я буду рядом так, чтобы вы могли дышать.*(нежно)*

**Харви:** А если вы снова забудете поесть — я буду рядом очень настойчиво.*(строго)*

В тишине вечернего города это звучит не как приказ, а как обещание быть рядом.

---

### eventHarveyCareMovementAnimationTest

*Hospital · `eventHarveyCareMovementAnimationTest`*

**Условия срабатывания**

- **Запуск:** vanilla event entry — войти в `Hospital` при выполнении условий ниже
- **CP preconditions:**
  - Время суток: 09:00–17:00
  - Не просмотрено событие `eventHarveyCareMovementAnimationTest`
  - *Сырой ключ:* `eventHarveyCareMovementAnimationTest/Time 900 1700/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyCareMovementAnimationTest`

**Харви:** Подождите... вы дрожите.*(грустно)*

Я просто немного устала...


**Харви:** Сядьте, пожалуйста. Я быстро проверю, всё ли в порядке.*(серьёзно)*

...хорошо.

**Харви:** Вот так. Дышите спокойно. Я рядом.*(с улыбкой)*

**Харви:** И после этого — чай. Это уже не обсуждается.

---
