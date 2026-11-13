# MedLink Internal Medicine Question Bank: Integration Guide

**Audience:** Front-end developers, Flutter developers, backend developers, AI/ML engineers, and medical reviewers.
**Files this guide describes:** `question_bank.json` (the data) and `questionnaire_tree.html` (a visual explorer of the same data).
**Bank version:** schema `1.0.0`, status `DRAFT_PENDING_PHYSICIAN_REVIEW`.

---

## 0. Read this first: safety and status 

1. **Nothing in this bank is clinically validated.** Every question, answer option, branching rule, emergency rule and numeric parameter is a *proposal* that carries `requiresPhysicianApproval: true` and `reviewStatus: "PENDING_PHYSICIAN_REVIEW"`. Do not use it with real patients until a licensed physician has reviewed and approved it.
2. **This is not a diagnostic tool.** It collects structured history to organize a consultation. Any AI output built on it is *decision support for a clinician*, never a final diagnosis.
3. **Show the urgent warning in the UI** (text is in `medicalSafety.urgentWarning`, Arabic and English). Patients with severe or rapidly worsening symptoms must be told to seek urgent care instead of finishing the questionnaire.
4. **Emergency rules override everything.** If an emergency rule fires, show the urgent alert immediately, before any AI result.
5. **Placeholder numbers.** Values with `parameterStatus: "PLACEHOLDER_REQUIRES_PHYSICIAN_DEFINITION"` (fever ≥ 7 days, cough ≥ 21 days) and the adult-scope age (18) are routing placeholders a physician must set or remove. The `min`/`max` values on NUMBER inputs are only input-validation limits, not clinical thresholds.

---

## 1. What is in the bank

| Item | Count |
|---|---|
| Symptoms | 19 |
| Questions | 157 |
| ↳ Common basic questions (`CQ_*`) | 10 |
| ↳ Symptom-specific questions | 143 |
| ↳ Shared questions (`SH_*`) | 4 |
| Answer options | 596 |
| Branching rules | 227 |
| Emergency rules | 27 |

Counts are also stored in `meta.counts`; read them from there rather than hard-coding.

**Symptoms** (IDs are `SYM_` plus the English name in upper case): Chest Pain, Shortness of Breath, Abdominal Pain, Headache, Fever, Cough, Dizziness, Palpitations, Fatigue, Nausea and Vomiting, Diarrhea, Constipation, Joint Pain, Back Pain, Swelling, Weight Loss, Loss of Appetite, Fainting, Jaundice.

**Question ID prefixes:** `CQ_` common, `SH_` shared, and two-letter symptom prefixes `CP SB AB HA FV CG DZ PL FT NV DR CN JP BK SW WL AP SY JD`.

**Question types:** `SINGLE_CHOICE` and `MULTIPLE_CHOICE` (the primary types), plus `NUMBER`, `DURATION`, `SCALE`.

---

## 2. The big picture: how a questionnaire runs

```
1. Patient selects one or more symptoms (order of selection matters)
2. COMMON questions   CQ_01 → … → CQ_10        (asked once)
        │  rule target START_SYMPTOM_PATHS
3. For each selected symptom, in order:
        entryQuestionId → … → rule target END_SYMPTOM_PATH
        (symptom path may jump into SHARED questions SH_01…SH_04)
4. After the last symptom path ends → questionnaire COMPLETE
5. Emergency rules are evaluated after EVERY answer and at completion
6. Send the collected answers to the backend → AI service
```

Special exit: if age is under 18 (`CQ_01`), a rule ends the questionnaire with reason `OUT_OF_SCOPE_AGE`.

**Key idea:** the *order of questions is data, not code.* The app never hard-codes "after question X comes Y". It asks the rule engine (Section 5) for the next step.

---

## 3. The JSON file structure

```
question_bank.json
├─ meta                  version, status, disclaimers, counts
├─ medicalSafety
│   ├─ urgentWarning              {en, ar}  ← show in UI
│   ├─ notDiagnosticDisclaimer    {en, ar}  ← show in UI
│   ├─ reviewPolicy
│   └─ emergencyRules             {status, evaluation, note, rules[27]}
├─ engine                execution model (flow, operators, answer formats, durationToDays)
├─ questionTypes[]
├─ bodySystems[]         id + Arabic + English names
├─ symptoms[]            the patient-selectable symptoms
├─ commonBasicQuestions  {entryQuestionId, questionIds[]}
├─ questions[]           every question (common, symptom, shared)
├─ answerOptions[]       every answer option (flat list, linked by questionId)
└─ branchingRules[]      every relationship between questions
```

### 3.1 Symptom object

```json
{
  "id": "SYM_CHEST_PAIN",
  "textEn": "Chest Pain", "textAr": "ألم الصدر",
  "bodySystemId": "CARDIOVASCULAR",
  "order": 1,
  "entryQuestionId": "CP_01",
  "symptomQuestionIds": ["CP_01", "..."],
  "sharedQuestionIds": ["SH_01", "SH_02", "SH_03"],
  "requiresPhysicianApproval": true,
  "reviewStatus": "PENDING_PHYSICIAN_REVIEW"
}
```

Use `entryQuestionId` to start that symptom's path. Use `order` for display ordering.

### 3.2 Question object

```json
{
  "id": "CP_02",
  "scope": "SYMPTOM",                 // COMMON | SYMPTOM | SHARED
  "symptomId": "SYM_CHEST_PAIN",      // null for common and shared
  "relatedSymptomIds": ["SYM_CHEST_PAIN"],
  "isShared": false,
  "bodySystemId": "CARDIOVASCULAR",
  "type": "SINGLE_CHOICE",
  "textEn": "How would you describe the pain?",
  "textAr": "كيف تصف الألم؟",
  "medicalPurpose": { "en": "...", "ar": "..." },
  "required": true,
  "answerOptionIds": ["CP_02_PRESSURE", "CP_02_SHARP", "..."],
  "inputSpec": null,                  // only for NUMBER / DURATION / SCALE
  "requiresPhysicianApproval": true,
  "reviewStatus": "PENDING_PHYSICIAN_REVIEW"
}
```

- Options are **not nested** inside the question. Look them up by `answerOptionIds`, or filter `answerOptions` by `questionId`.
- `medicalPurpose` is "why we ask this." Show it as an optional "Why am I being asked this?" hint.
- `required: false` means the patient may skip. Skipped questions simply have no answer (see Section 5.4).

### 3.3 Answer option object

```json
{
  "id": "CP_02_PRESSURE",           // always <questionId>_<KEY>
  "questionId": "CP_02",
  "order": 1,
  "textAr": "ضغط أو ثقل أو انقباض",
  "textEn": "Pressure, heaviness or tightness",
  "exclusive": false                // true for "None of these" style options
}
```

`exclusive: true` options (keys `NONE`, `NONE_*`) must clear all other selections in a MULTIPLE_CHOICE question, and selecting any other option must clear the exclusive one.

### 3.4 `inputSpec` for non-choice questions

| Type | `inputSpec` | Answer you store |
|---|---|---|
| NUMBER | `{unit, min, max, integerOnly}` | `{ "value": 30 }` |
| DURATION | `{units[], valueMin, normalizedUnit:"DAYS"}` | `{ "value": 3, "unit": "WEEKS" }` |
| SCALE | `{min, max, step, labelMinEn/Ar, labelMaxEn/Ar}` | `{ "value": 7 }` |

DURATION units are `MINUTES, HOURS, DAYS, WEEKS, MONTHS, YEARS`. Conversion table (also in `engine.durationToDays`): minute = 1/1440, hour = 1/24, day = 1, week = 7, month = 30, year = 365.

### 3.5 Shared questions

`SH_01` (pain severity 0–10), `SH_02` (clot-risk screen), `SH_03` ("getting worse quickly?"), `SH_04` ("fever or chills?"). They are `scope: "SHARED"` and list every symptom that uses them in `relatedSymptomIds`.

Behavior rules:
- A shared question is **asked once per session**. If a second selected symptom reaches it, **reuse the stored answer** and just evaluate its rules for the *active* symptom.
- Its branching rules are written per symptom using the condition `ACTIVE_SYMPTOM_IS`, because where you go next depends on which symptom path you are in.

---

## 4. Branching rules

### 4.1 Rule object

```json
{
  "id": "BR_0042",
  "sourceQuestionId": "CP_04",
  "priority": 10,
  "condition": { "op": "AND", "conditions": [
      { "op": "ANSWER_ANY_OF", "questionId": "CP_03", "optionIds": ["CP_03_LEFT_ARM", "CP_03_JAW_NECK"] },
      { "op": "ANSWER_ANY_OF", "questionId": "CP_04", "optionIds": ["CP_04_EXERTION", "CP_04_REST"] }
  ]},
  "target": { "type": "QUESTION", "questionId": "CP_08" },
  "note": "Multi-answer AND rule across two questions",
  "requiresPhysicianApproval": true,
  "reviewStatus": "PENDING_PHYSICIAN_REVIEW"
}
```

A rule means: *after the patient answers `sourceQuestionId`, if `condition` is true, go to `target`.*

### 4.2 Selecting the next step

1. Take all rules whose `sourceQuestionId` is the question just answered.
2. Sort by `priority` ascending (**lower number = checked first**).
3. The **first rule whose condition is true wins.** Stop there.
4. Every non-shared question has a default rule `{op:"ALWAYS"}` with priority `1000` as the fallback. Shared questions have one rule per symptom instead.

### 4.3 Condition operators

| `op` | Fields | True when |
|---|---|---|
| `ALWAYS` | none | always (default path) |
| `ANSWER_EQUALS` | `questionId`, `optionId` | that option is among the answer's selected options |
| `ANSWER_ANY_OF` | `questionId`, `optionIds[]` | **at least one** listed option is selected (multi-answer OR) |
| `ANSWER_ALL_OF` | `questionId`, `optionIds[]` | **all** listed options are selected (multi-answer AND) |
| `NUMBER_COMPARE` | `questionId`, `operator`, `value` | numeric answer compares true |
| `DURATION_COMPARE` | `questionId`, `operator`, `valueDays` | duration converted to days compares true |
| `SCALE_COMPARE` | `questionId`, `operator`, `value` | supported, not used in this draft |
| `ACTIVE_SYMPTOM_IS` | `symptomId` | the symptom path currently running |
| `SYMPTOM_SELECTED` | `symptomId` | the patient selected that symptom (used by emergency rules) |
| `AND` / `OR` | `conditions[]` | all / any child is true |
| `NOT` | `condition` | child is false |

`operator` is one of `LT, LTE, GT, GTE, EQ`.

Conditions may reference **other questions**, not just the source question (cross-question rules). Example: `DZ_05` checks the common answer `CQ_05` (current medicines) to decide whether to ask `DZ_06`.

### 4.4 Targets

| `target.type` | Meaning |
|---|---|
| `QUESTION` | go to `questionId` |
| `START_SYMPTOM_PATHS` | common questions are finished; begin the first selected symptom |
| `END_SYMPTOM_PATH` | this symptom path is finished; begin next selected symptom, or complete if none left |
| `END_QUESTIONNAIRE` | stop now with `reason` (e.g. `OUT_OF_SCOPE_AGE`) |

### 4.5 Patterns used in the bank

- **Single-answer branch:** `ANSWER_EQUALS` (e.g. pregnancy question only if sex is female).
- **Multi-answer branch:** `ANSWER_ANY_OF` / `ANSWER_ALL_OF`.
- **AND/OR conditions:** nested `conditions[]`, sometimes across two questions.
- **Skipping:** a rule whose target is a *later* question bypasses the ones in between (e.g. thunderclap headache jumps straight to red-flag symptoms).
- **Numeric / duration:** age under 18; fever ≥ 7 days; cough ≥ 21 days.
- **Shared paths:** `ACTIVE_SYMPTOM_IS` plus `SYMPTOM_SELECTED`.
- **Multiple paths:** many decision points produce different routes to the same end.
- **Completion:** `END_SYMPTOM_PATH` and `END_QUESTIONNAIRE` targets.

The bank was checked to have **no circular paths** and every question is reachable.

---

## 5. Implementing the engine

The engine is small. You implement the same logic in the app (for the live questionnaire) and, ideally, again on the backend (to re-verify what the app sent).

### 5.1 Session state

```
selected:       [symptomId, ...]     order of selection
activeSymptom:  symptomId | null
symptomIndex:   int
current:        questionId | null
answers:        { questionId: Answer }
path:           [ {questionId, ruleId} ]   ordered history
alerts:         [emergencyRuleId, ...]
```

`Answer` is one of: `{options:[optionId,…]}`, `{value:number}`, `{value:number, unit:"DAYS"}`.

**An unanswered question makes any condition that references it evaluate to `false`** (and `NOT` is applied afterwards).

### 5.2 Algorithm

```
start():
    current = "CQ_01"
    settle()

submit(answer):                        # answer may be null when skipping an optional question
    if answer: answers[current] = answer
    rule = firstTrueRule(current)      # sorted by priority
    checkEmergencyRules()
    path.push({current, rule.id})
    move(rule.target)
    settle()

move(target):
    QUESTION             → current = target.questionId
    START_SYMPTOM_PATHS  → nextSymptom()
    END_SYMPTOM_PATH     → nextSymptom()
    END_QUESTIONNAIRE    → finish(target.reason)

nextSymptom():
    symptomIndex++
    if symptomIndex >= selected.length → finish("COMPLETED")
    else activeSymptom = selected[symptomIndex]; current = entryQuestionId

settle():                              # skip shared questions already answered
    while current is a shared question AND answers[current] exists:
        rule = firstTrueRule(current)  # uses activeSymptom
        path.push(...)
        move(rule.target)
    checkEmergencyRules()
```

Order of the first call: after `selected` is set, `start()` begins at `CQ_01`; `CQ_10`'s rule `START_SYMPTOM_PATHS` moves into the first symptom.

### 5.3 JavaScript / TypeScript reference engine (works in a browser or Node)

```js
// engine.js: loads question_bank.json and runs the questionnaire
const DAYS = {}; // filled from bank.engine.durationToDays

function cmp(a, op, b) {
  switch (op) {
    case 'LT': return a < b;  case 'LTE': return a <= b;
    case 'GT': return a > b;  case 'GTE': return a >= b;
    case 'EQ': return a === b;
  }
  return false;
}

function evalCond(c, s) {
  const a = c.questionId ? s.answers[c.questionId] : undefined;
  switch (c.op) {
    case 'ALWAYS': return true;
    case 'ANSWER_EQUALS':  return !!(a && a.options && a.options.includes(c.optionId));
    case 'ANSWER_ANY_OF':  return !!(a && a.options && c.optionIds.some(o => a.options.includes(o)));
    case 'ANSWER_ALL_OF':  return !!(a && a.options && c.optionIds.every(o => a.options.includes(o)));
    case 'NUMBER_COMPARE':
    case 'SCALE_COMPARE':  return !!(a && typeof a.value === 'number' && cmp(a.value, c.operator, c.value));
    case 'DURATION_COMPARE':
      return !!(a && typeof a.value === 'number' && cmp(a.value * DAYS[a.unit], c.operator, c.valueDays));
    case 'ACTIVE_SYMPTOM_IS': return s.activeSymptom === c.symptomId;
    case 'SYMPTOM_SELECTED':  return s.selected.includes(c.symptomId);
    case 'AND': return c.conditions.every(x => evalCond(x, s));
    case 'OR':  return c.conditions.some(x => evalCond(x, s));
    case 'NOT': return !evalCond(c.condition, s);
  }
  throw new Error('Unknown op ' + c.op);
}

class Questionnaire {
  constructor(bank, selectedSymptomIds) {
    this.bank = bank;
    Object.assign(DAYS, bank.engine.durationToDays);
    this.q = Object.fromEntries(bank.questions.map(q => [q.id, q]));
    this.rulesBySource = {};
    for (const r of bank.branchingRules) (this.rulesBySource[r.sourceQuestionId] ??= []).push(r);
    for (const k in this.rulesBySource) this.rulesBySource[k].sort((a, b) => a.priority - b.priority);
    this.sym = Object.fromEntries(bank.symptoms.map(s => [s.id, s]));
    this.s = { selected: selectedSymptomIds, activeSymptom: null, answers: {} };
    this.symptomIndex = -1;
    this.current = 'CQ_01';
    this.path = [];
    this.alerts = [];
    this.done = false;
    this.endReason = null;
    this._settle();
  }
  currentQuestion() { return this.done ? null : this.q[this.current]; }
  options(qid) { return this.bank.answerOptions.filter(o => o.questionId === qid); }

  _firstTrueRule(qid) {
    return (this.rulesBySource[qid] || []).find(r => evalCond(r.condition, this.s)) || null;
  }
  _checkEmergency() {
    for (const e of this.bank.medicalSafety.emergencyRules.rules)
      if (!this.alerts.includes(e.id) && evalCond(e.condition, this.s)) this.alerts.push(e.id);
  }
  _nextSymptom() {
    this.symptomIndex++;
    if (this.symptomIndex >= this.s.selected.length) { this.done = true; this.endReason = 'COMPLETED'; this.s.activeSymptom = null; return; }
    this.s.activeSymptom = this.s.selected[this.symptomIndex];
    this.current = this.sym[this.s.activeSymptom].entryQuestionId;
  }
  _move(t) {
    if (t.type === 'QUESTION') this.current = t.questionId;
    else if (t.type === 'START_SYMPTOM_PATHS' || t.type === 'END_SYMPTOM_PATH') this._nextSymptom();
    else { this.done = true; this.endReason = t.reason || 'END'; }
  }
  _settle() {
    let guard = 0;
    while (!this.done && guard++ < 500) {
      const q = this.q[this.current];
      if (q.isShared && this.s.answers[q.id] !== undefined) {   // already answered, reuse
        const r = this._firstTrueRule(q.id);
        if (!r) { this.done = true; this.endReason = 'NO_RULE_MATCHED'; break; }
        this.path.push({ questionId: q.id, ruleId: r.id, auto: true });
        this._move(r.target);
      } else break;
    }
    this._checkEmergency();
  }
  /** answer: {options:[...]} | {value} | {value, unit} | null (skip an optional question) */
  submit(answer) {
    if (this.done) return;
    const q = this.q[this.current];
    if (answer == null && q.required) throw new Error(q.id + ' is required');
    if (answer) this.s.answers[q.id] = answer;
    const r = this._firstTrueRule(q.id);
    this._checkEmergency();
    this.path.push({ questionId: q.id, ruleId: r ? r.id : null });
    if (!r) { this.done = true; this.endReason = 'NO_RULE_MATCHED'; return; }
    this._move(r.target);
    this._settle();
  }
}
module.exports = { Questionnaire, evalCond };
```

### 5.4 Edge cases to handle in the UI

- **Skipping optional questions:** call `submit(null)`. No answer is stored; rules that reference it evaluate to false, so the default path is taken.
- **Required questions:** disable "Next" until answered.
- **Exclusive options:** if the patient ticks "None of these", untick everything else, and vice versa.
- **Going back:** keep a stack of snapshots (`answers`, `current`, `symptomIndex`, `path`, `alerts`). On "Back", restore the snapshot. Do **not** try to un-run rules; restore state instead. When an earlier answer changes, discard the answers of every later question in the path and re-run from there, otherwise stale answers can leak into conditions.
- **Several symptoms:** selected symptom order = path order. Shared questions already answered are skipped automatically.
- **Out of scope:** when `endReason === "OUT_OF_SCOPE_AGE"`, show a message that the service is for adults, and do **not** send anything to the AI.

---

## 6. Front-end (web) usage

### 6.1 Loading the data

```js
const bank = await (await fetch('/assets/question_bank.json')).json();
```

Cache it and keep it **versioned** (`meta.schemaVersion`). Do not edit the JSON by hand in the front end.

### 6.2 Screens

1. **Disclaimer + urgent warning** (from `medicalSafety`). Require acknowledgement.
2. **Symptom picker:** a grid or list of `bank.symptoms` using `textAr`/`textEn`, multi-select.
3. **Question screen:** one question at a time, with progress that says "Question N" rather than "N of M" (the total length depends on answers).
4. **Review screen:** show all answers, allow editing (see "going back" above).
5. **Result screen:** only what the physician-approved product design allows (see Section 9).

### 6.3 Rendering by question type

| Type | Control |
|---|---|
| SINGLE_CHOICE | radio list or large tappable cards |
| MULTIPLE_CHOICE | checkbox list, exclusive options clear the others |
| NUMBER | numeric input with unit label; enforce `min`/`max`/`integerOnly` |
| DURATION | number input + unit dropdown from `inputSpec.units` |
| SCALE | slider/stepper from `min` to `max` with min/max labels |

Always render options in `order`. Render both languages according to the user's language; Arabic needs `dir="rtl"`.

### 6.4 Emergency alerts

After every `submit`, if `alerts` contains a new ID, show a blocking banner with `messageEn`/`messageAr` from that emergency rule and a button to call the local emergency number. Let the patient choose to continue the questionnaire, but keep the alert visible and include the alert IDs in the final payload.

### 6.5 Using `questionnaire_tree.html`

Open it directly in a browser (no server, no internet). Use it to:
- **Home:** see all symptoms; tick several and press *Simulate*.
- **Symptom tree:** expand/collapse branches, click any question to see ID, Arabic/English text, type, purpose, answers, related symptom, body system, required status, all branching rules, and "next question by answer".
- **Search** by question ID or text. **Shared questions** are highlighted in purple.
- **Path simulator:** answer questions, see the full path and triggered emergency rules. It uses the same engine logic as Section 5.
- **Emergency assessment** and **Statistics** pages.

It is a review tool for developers and doctors, not a patient-facing screen.

---

## 7. Flutter usage

### 7.1 Add the asset

```yaml
# pubspec.yaml
flutter:
  assets:
    - assets/question_bank.json
```

### 7.2 Minimal models

```dart
class Bank {
  final Map<String, dynamic> raw;
  final Map<String, Map<String, dynamic>> questions;
  final Map<String, List<Map<String, dynamic>>> options;       // by questionId
  final Map<String, List<Map<String, dynamic>>> rulesBySource; // sorted by priority
  final Map<String, Map<String, dynamic>> symptoms;
  final Map<String, double> daysPer;

  Bank._(this.raw, this.questions, this.options, this.rulesBySource, this.symptoms, this.daysPer);

  factory Bank.fromJson(Map<String, dynamic> j) {
    final qs = {for (final q in j['questions']) q['id'] as String: Map<String, dynamic>.from(q)};
    final opts = <String, List<Map<String, dynamic>>>{};
    for (final o in j['answerOptions']) {
      (opts[o['questionId']] ??= []).add(Map<String, dynamic>.from(o));
    }
    opts.forEach((_, l) => l.sort((a, b) => (a['order'] as int).compareTo(b['order'] as int)));
    final rules = <String, List<Map<String, dynamic>>>{};
    for (final r in j['branchingRules']) {
      (rules[r['sourceQuestionId']] ??= []).add(Map<String, dynamic>.from(r));
    }
    rules.forEach((_, l) => l.sort((a, b) => (a['priority'] as int).compareTo(b['priority'] as int)));
    final sy = {for (final s in j['symptoms']) s['id'] as String: Map<String, dynamic>.from(s)};
    final d = (j['engine']['durationToDays'] as Map).map((k, v) => MapEntry(k as String, (v as num).toDouble()));
    return Bank._(j, qs, opts, rules, sy, d);
  }
}

class Answer {
  final List<String>? options;
  final num? value;
  final String? unit;
  Answer.options(this.options) : value = null, unit = null;
  Answer.number(this.value, {this.unit}) : options = null;
  Map<String, dynamic> toJson() => options != null
      ? {'options': options}
      : {'value': value, if (unit != null) 'unit': unit};
}
```

Load it once:

```dart
final bank = Bank.fromJson(jsonDecode(await rootBundle.loadString('assets/question_bank.json')));
```

### 7.3 Engine in Dart

```dart
class Session {
  final Bank bank;
  final List<String> selected;
  String? activeSymptom;
  int symptomIndex = -1;
  String? current = 'CQ_01';
  final Map<String, Answer> answers = {};
  final List<Map<String, dynamic>> path = [];
  final List<String> alerts = [];
  bool done = false;
  String? endReason;

  Session(this.bank, this.selected) { _settle(); }

  bool _cmp(num a, String op, num b) {
    switch (op) {
      case 'LT': return a < b;
      case 'LTE': return a <= b;
      case 'GT': return a > b;
      case 'GTE': return a >= b;
      case 'EQ': return a == b;
    }
    return false;
  }

  bool eval(Map c) {
    final a = c['questionId'] != null ? answers[c['questionId']] : null;
    switch (c['op']) {
      case 'ALWAYS': return true;
      case 'ANSWER_EQUALS': return a?.options?.contains(c['optionId']) ?? false;
      case 'ANSWER_ANY_OF':
        return a?.options != null && (c['optionIds'] as List).any((o) => a!.options!.contains(o));
      case 'ANSWER_ALL_OF':
        return a?.options != null && (c['optionIds'] as List).every((o) => a!.options!.contains(o));
      case 'NUMBER_COMPARE':
      case 'SCALE_COMPARE':
        return a?.value != null && _cmp(a!.value!, c['operator'], c['value']);
      case 'DURATION_COMPARE':
        return a?.value != null && _cmp(a!.value! * bank.daysPer[a.unit]!, c['operator'], c['valueDays']);
      case 'ACTIVE_SYMPTOM_IS': return activeSymptom == c['symptomId'];
      case 'SYMPTOM_SELECTED': return selected.contains(c['symptomId']);
      case 'AND': return (c['conditions'] as List).every((x) => eval(x));
      case 'OR': return (c['conditions'] as List).any((x) => eval(x));
      case 'NOT': return !eval(c['condition']);
    }
    throw StateError('Unknown op ${c['op']}');
  }

  Map<String, dynamic>? _firstRule(String qid) {
    for (final r in bank.rulesBySource[qid] ?? const []) {
      if (eval(r['condition'])) return r;
    }
    return null;
  }

  void _checkEmergency() {
    for (final e in bank.raw['medicalSafety']['emergencyRules']['rules']) {
      if (!alerts.contains(e['id']) && eval(e['condition'])) alerts.add(e['id']);
    }
  }

  void _nextSymptom() {
    symptomIndex++;
    if (symptomIndex >= selected.length) { done = true; endReason = 'COMPLETED'; activeSymptom = null; current = null; return; }
    activeSymptom = selected[symptomIndex];
    current = bank.symptoms[activeSymptom]!['entryQuestionId'];
  }

  void _move(Map t) {
    switch (t['type']) {
      case 'QUESTION': current = t['questionId']; break;
      case 'START_SYMPTOM_PATHS':
      case 'END_SYMPTOM_PATH': _nextSymptom(); break;
      default: done = true; endReason = t['reason'] ?? 'END'; current = null;
    }
  }

  void _settle() {
    var guard = 0;
    while (!done && current != null && guard++ < 500) {
      final q = bank.questions[current]!;
      if (q['isShared'] == true && answers.containsKey(current)) {
        final r = _firstRule(current!);
        if (r == null) { done = true; endReason = 'NO_RULE_MATCHED'; break; }
        path.add({'questionId': current, 'ruleId': r['id'], 'auto': true});
        _move(r['target']);
      } else { break; }
    }
    _checkEmergency();
  }

  void submit(Answer? a) {
    final qid = current!;
    final q = bank.questions[qid]!;
    if (a == null && q['required'] == true) throw StateError('$qid is required');
    if (a != null) answers[qid] = a;
    final r = _firstRule(qid);
    _checkEmergency();
    path.add({'questionId': qid, 'ruleId': r?['id']});
    if (r == null) { done = true; endReason = 'NO_RULE_MATCHED'; current = null; return; }
    _move(r['target']);
    _settle();
  }
}
```

### 7.4 UI mapping in Flutter

```dart
Widget buildInput(Map q, List<Map> opts, bool isAr) {
  switch (q['type']) {
    case 'SINGLE_CHOICE':   // RadioListTile per option
    case 'MULTIPLE_CHOICE': // CheckboxListTile per option, honor option['exclusive']
    case 'NUMBER':          // TextField(keyboardType: number), validate inputSpec min/max
    case 'DURATION':        // TextField + DropdownButton(units)
    case 'SCALE':           // Slider(min,max,divisions: max-min)
  }
  // Text: isAr ? q['textAr'] : q['textEn']
}
```

Arabic: wrap the app in `Directionality(textDirection: TextDirection.rtl, …)` or set the `Locale('ar')`, and use an Arabic-capable font.

### 7.5 State management and back navigation

Keep `Session` inside a `ChangeNotifier`/Riverpod/Bloc. For "Back", keep a stack of deep copies of `(answers, current, symptomIndex, activeSymptom, path, alerts, done, endReason)` taken **before** each `submit`, and restore the last one. When the patient changes an earlier answer, truncate to that snapshot and re-run forward.

### 7.6 Offline and sync

The bank is static, so the whole questionnaire can run offline. Save an in-progress session locally (answers + selected symptoms are enough, since path can be recomputed) and upload when online. Always store `bankVersion` with the session.

---

## 8. Sending the answers to the AI model

### 8.1 Architecture

```
Flutter / Web app ──HTTPS──▶ Your Backend API ──▶ AI service (model)
                                  │
                                  ├─ re-validates answers with the same engine
                                  ├─ stores session + payload (encrypted)
                                  └─ returns a *reviewed-safe* response
```

- **Never call the AI model directly from the app.** Keys, prompts and patient data must stay on the backend.
- The backend should **re-run the engine** on the submitted answers to confirm the path is valid and to recompute emergency alerts itself, rather than trusting the client.

### 8.2 Recommended request payload

Send **IDs for machines and text for humans/LLMs**. IDs are stable and compact; text makes the data self-explanatory for a language model and for audit.

```json
{
  "sessionId": "b6f1d1e0-...",
  "bank": { "schemaVersion": "1.0.0", "status": "DRAFT_PENDING_PHYSICIAN_REVIEW" },
  "language": "ar",
  "completedAt": "2026-11-13T10:30:00Z",
  "endReason": "COMPLETED",
  "selectedSymptoms": ["SYM_CHEST_PAIN", "SYM_SHORTNESS_OF_BREATH"],
  "demographics": { "ageYears": 54, "sex": "MALE", "pregnant": null },
  "emergencyAlertsTriggered": ["EM_01"],
  "answers": [
    {
      "questionId": "CQ_04",
      "scope": "COMMON",
      "type": "MULTIPLE_CHOICE",
      "bodySystemId": "GENERAL",
      "questionEn": "Do you have any chronic diseases?",
      "selected": [
        { "optionId": "CQ_04_DM",  "textEn": "Diabetes" },
        { "optionId": "CQ_04_HTN", "textEn": "High blood pressure" }
      ]
    },
    {
      "questionId": "CP_02",
      "scope": "SYMPTOM",
      "symptomId": "SYM_CHEST_PAIN",
      "type": "SINGLE_CHOICE",
      "questionEn": "How would you describe the pain?",
      "selected": [ { "optionId": "CP_02_PRESSURE", "textEn": "Pressure, heaviness or tightness" } ]
    },
    {
      "questionId": "CP_06",
      "scope": "SYMPTOM",
      "symptomId": "SYM_CHEST_PAIN",
      "type": "DURATION",
      "questionEn": "How long does each episode of pain last?",
      "value": 20, "unit": "MINUTES", "valueInDays": 0.0139
    },
    {
      "questionId": "SH_01",
      "scope": "SHARED",
      "type": "SCALE",
      "usedBySymptoms": ["SYM_CHEST_PAIN"],
      "questionEn": "Pain severity 0–10",
      "value": 8
    }
  ],
  "pathTaken": ["CQ_01", "CQ_02", "CQ_04", "CP_01", "CP_02"],
  "skippedOptionalQuestions": ["CQ_08"]
}
```

Guidelines:
- **Always include** `bank.schemaVersion`, `selectedSymptoms`, demographics, `emergencyAlertsTriggered`, and every answer.
- Add a derived **`valueInDays`** for DURATION so the model gets a comparable number.
- Mark **skipped optional questions** explicitly. "Not answered" and "No" are different.
- Keep answers in **path order**. The order carries meaning.
- Use **English text** for the model even when the patient used Arabic (include Arabic only if the model is Arabic-capable). The option IDs keep the data unambiguous either way.
- Keep **shared** answers once, tagged with the symptoms that used them.
- Remove direct identifiers (name, phone, national ID) from the AI payload. Use a pseudonymous `sessionId`.

### 8.3 Building the payload (JavaScript)

```js
function buildPayload(bank, qn, demographics, language) {
  const optById = Object.fromEntries(bank.answerOptions.map(o => [o.id, o]));
  const answers = Object.entries(qn.s.answers).map(([qid, a]) => {
    const q = qn.q[qid];
    const base = { questionId: qid, scope: q.scope, type: q.type, bodySystemId: q.bodySystemId, questionEn: q.textEn };
    if (q.symptomId) base.symptomId = q.symptomId;
    if (q.isShared) base.usedBySymptoms = q.relatedSymptomIds.filter(s => qn.s.selected.includes(s));
    if (a.options) base.selected = a.options.map(id => ({ optionId: id, textEn: optById[id].textEn }));
    else { base.value = a.value; if (a.unit) { base.unit = a.unit; base.valueInDays = a.value * bank.engine.durationToDays[a.unit]; } }
    return base;
  });
  return {
    sessionId: crypto.randomUUID(),
    bank: { schemaVersion: bank.meta.schemaVersion, status: bank.meta.status },
    language,
    completedAt: new Date().toISOString(),
    endReason: qn.endReason,
    selectedSymptoms: qn.s.selected,
    demographics,
    emergencyAlertsTriggered: qn.alerts,
    answers,
    pathTaken: qn.path.map(p => p.questionId)
  };
}
```

### 8.4 Building the payload (Dart)

```dart
Map<String, dynamic> buildPayload(Bank bank, Session s, Map<String, dynamic> demographics, String lang) {
  final optById = {for (final l in bank.options.values) for (final o in l) o['id'] as String: o};
  final out = <Map<String, dynamic>>[];
  s.answers.forEach((qid, a) {
    final q = bank.questions[qid]!;
    final m = <String, dynamic>{
      'questionId': qid, 'scope': q['scope'], 'type': q['type'],
      'bodySystemId': q['bodySystemId'], 'questionEn': q['textEn'],
      if (q['symptomId'] != null) 'symptomId': q['symptomId'],
    };
    if (a.options != null) {
      m['selected'] = a.options!.map((id) => {'optionId': id, 'textEn': optById[id]!['textEn']}).toList();
    } else {
      m['value'] = a.value;
      if (a.unit != null) { m['unit'] = a.unit; m['valueInDays'] = a.value! * bank.daysPer[a.unit]!; }
    }
    out.add(m);
  });
  return {
    'bank': {'schemaVersion': bank.raw['meta']['schemaVersion'], 'status': bank.raw['meta']['status']},
    'language': lang,
    'completedAt': DateTime.now().toUtc().toIso8601String(),
    'endReason': s.endReason,
    'selectedSymptoms': s.selected,
    'demographics': demographics,
    'emergencyAlertsTriggered': s.alerts,
    'answers': out,
    'pathTaken': s.path.map((p) => p['questionId']).toList(),
  };
}
// POST to YOUR backend, not the AI provider:
// final res = await http.post(Uri.parse('$base/api/v1/assessments'),
//     headers: {'Content-Type': 'application/json', 'Authorization': 'Bearer $token'},
//     body: jsonEncode(payload));
```

### 8.5 Backend responsibilities

1. **Authenticate** the patient; accept the payload over HTTPS only.
2. **Re-validate:** every `questionId`/`optionId` exists; option belongs to its question; single-choice has exactly one option; number is within `inputSpec`; required questions are answered; the claimed `pathTaken` matches what the engine produces from the answers.
3. **Recompute emergency rules** server-side. If the server computes an alert the client did not, trust the server.
4. **Store** the raw payload with `bank.schemaVersion`, timestamp and the user ID (encrypted at rest).
5. **Call the AI service** with the pseudonymized payload.
6. **Post-process** the model output (Section 8.7) and return only what the product is approved to show.
7. **Audit-log** the payload hash, model name/version, output and who (if anyone) reviewed it.

### 8.6 Prompting the model

Give the model a role, the safety constraints, the data, and a strict output schema. Example system prompt:

```
You are a clinical decision-support assistant for Internal Medicine triage. You assist a licensed physician.
You do NOT diagnose and you do NOT give treatment instructions to the patient.
Input: a JSON object of structured questionnaire answers (schema version in "bank").
Rules:
- Use only the information provided. Treat unanswered questions as unknown, not as "no".
- If "emergencyAlertsTriggered" is non-empty, set urgency to "EMERGENCY" regardless of anything else.
- Give possible conditions as hypotheses for physician review, ranked, with the answer IDs that support each.
- List information that is missing and would change your assessment.
- Return ONLY valid JSON matching the output schema. No prose outside the JSON.
```

### 8.7 Recommended response schema from the AI

```json
{
  "urgency": "EMERGENCY | URGENT | ROUTINE | UNDETERMINED",
  "urgencyReason": "short text",
  "suggestedSpecialty": "Cardiology",
  "hypotheses": [
    {
      "condition": "text",
      "confidence": "LOW | MEDIUM | HIGH",
      "supportingAnswerIds": ["CP_02_PRESSURE", "CP_04_EXERTION"],
      "againstAnswerIds": []
    }
  ],
  "missingInformation": ["Is the pain present now?"],
  "suggestedQuestionsForDoctor": ["..."],
  "requiresPhysicianReview": true,
  "disclaimer": "Decision support only. Not a diagnosis."
}
```

Backend post-processing rules:
- **Validate** the JSON against this schema. Reject and retry on invalid output.
- **Force `requiresPhysicianReview: true`** and the disclaimer.
- **Urgency floor:** if any emergency rule fired, urgency can never be lower than `EMERGENCY`, whatever the model said.
- Check every `supportingAnswerIds` entry actually exists in the submitted answers (drops hallucinated evidence).
- **Do not show raw hypotheses to patients.** Route the result to a physician's dashboard. Patient-facing text should be limited to a physician-approved urgency message and specialty suggestion.

### 8.8 Do this, not that

| Do | Don't |
|---|---|
| Send question and option **IDs plus text** | Send only free text the model must interpret |
| Mark skipped/unknown explicitly | Treat missing answers as "No" |
| Let emergency rules set the floor on urgency | Let the AI downgrade an emergency alert |
| Keep a physician in the loop | Show diagnoses to patients automatically |
| Pseudonymize, encrypt, log | Send names/phone numbers to the model |
| Version every session with `schemaVersion` | Change the bank without bumping the version |

---

## 9. Training or evaluating the AI model on this data

- **Features:** each option ID can be one feature (one-hot / multi-hot for multi-choice). NUMBER and SCALE are numeric. DURATION should use `valueInDays`. Missing = separate "unknown" state, not 0.
- **Labels** must come from physician-confirmed outcomes (final diagnosis, specialty, urgency). The questionnaire alone has no labels. Do not train on the model's own outputs.
- **Variable length:** different patients answer different questions. Use models that handle missing features (tree ensembles, or an LLM over the JSON payload), not a fixed-length vector that treats unasked questions as "No".
- **Validate** on a held-out set with physician review, by symptom and by urgency class. Measure under-triage (missed emergencies) first. It is the dangerous error.
- **Re-validate after every change to the bank**; new or removed questions change the feature set.

---

## 10. Changing the bank safely

1. Edit the data (not the engine). A new question needs: unique `id`, both texts, `type`, `bodySystemId`, `medicalPurpose`, `required`, options (if choice), and at least a default rule `ALWAYS` priority `1000` (or one rule per symptom if shared).
2. Option IDs must be `<questionId>_<KEY>`.
3. After every edit run these checks: all IDs unique; every rule target exists; every `optionId` in a condition belongs to the referenced question; every question reachable; no cycles; every non-shared question has a default rule; every shared question has a rule for each symptom using it.
4. Reset `reviewStatus` to `PENDING_PHYSICIAN_REVIEW` for anything changed, and bump `meta.schemaVersion`.
5. Regenerate `questionnaire_tree.html` from the same data so reviewers see exactly what ships.

---

## 11. Testing checklist

**Engine**
- [ ] Each symptom alone completes from `CQ_01` to `COMPLETED`.
- [ ] Two and three symptoms together: shared questions asked once, path order = selection order.
- [ ] Age < 18 ends with `OUT_OF_SCOPE_AGE`.
- [ ] Female shows `CQ_03`; male skips it. "No allergy" skips `CQ_07`.
- [ ] Skipping an optional question follows the default path.
- [ ] Exclusive options ("None") behave correctly.
- [ ] Back/edit recomputes the path without stale answers.

**Emergency**
- [ ] Each of the 27 emergency rules can be triggered by a crafted answer set.
- [ ] The urgent alert is shown immediately and appears in the payload.

**Integration**
- [ ] Payload validates on the backend; tampered `pathTaken` is rejected.
- [ ] Arabic and English render; RTL layout correct.
- [ ] AI output failing the schema is rejected and retried.
- [ ] An emergency alert always yields urgency `EMERGENCY`.

---

## 12. Quick reference

**Questionnaire order:** `CQ_01 → CQ_10 → [symptom path A → symptom path B …] → complete`

**Common questions:** CQ_01 age · CQ_02 sex · CQ_03 pregnancy (females) · CQ_04 chronic diseases · CQ_05 current medicines · CQ_06 medication allergy (yes/no) · CQ_07 allergy details · CQ_08 smoking (optional) · CQ_09 symptom duration · CQ_10 onset (sudden/gradual).

**Shared questions:** SH_01 pain severity · SH_02 clot-risk screen · SH_03 rapid worsening · SH_04 fever/chills.

**Rule evaluation:** lower `priority` first, first true condition wins, unanswered = false.

**Answer shapes:** choice `{options:[ids]}` · number `{value}` · duration `{value, unit}` · scale `{value}`.

**Golden rules:** the data drives the flow; emergency rules override AI; a physician reviews all AI output; nothing here is validated until a doctor approves it.
