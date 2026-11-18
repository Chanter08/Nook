import type { PlantPayload } from "@/types/plant";

type Conditions = Pick<PlantPayload, "growingIn" | "rainExposure" | "sunExposure">;

interface Props {
  value: Conditions;
  onChange: (patch: Partial<Conditions>) => void;
  disabled?: boolean;
}

const selectClass = "mt-1 h-11 w-full rounded-2xl border border-stone-200 bg-stone-50 px-3 text-sm dark:border-white/10 dark:bg-white/5";

export default function PlantGrowingConditions({ value, onChange, disabled = false }: Props) {
  return (
    <fieldset disabled={disabled} className="mt-4 rounded-3xl border border-emerald-200 p-4 dark:border-emerald-400/20">
      <legend className="px-2 text-sm font-semibold">Growing conditions</legend>
      <p className="mb-3 text-xs text-stone-500 dark:text-stone-400">
        These details help Nook avoid assuming outdoor rainfall is watering a sheltered pot.
      </p>
      <div className="grid gap-3 sm:grid-cols-3">
        <label className="text-xs font-medium">
          Growing in
          <select className={selectClass} value={value.growingIn ?? ""} onChange={(event) => onChange({ growingIn: event.target.value as Conditions["growingIn"] || null })}>
            <option value="">Not sure</option>
            <option value="pot">Pot / container</option>
            <option value="ground">Ground</option>
          </select>
        </label>
        <label className="text-xs font-medium">
          Rain reaches plant?
          <select className={selectClass} value={value.rainExposure ?? ""} onChange={(event) => onChange({ rainExposure: event.target.value as Conditions["rainExposure"] || null })}>
            <option value="">Not sure</option>
            <option value="exposed">Yes, outdoors</option>
            <option value="sheltered">No, sheltered</option>
            <option value="indoor">No, indoors</option>
          </select>
        </label>
        <label className="text-xs font-medium">
          Sun exposure
          <select className={selectClass} value={value.sunExposure ?? ""} onChange={(event) => onChange({ sunExposure: event.target.value as Conditions["sunExposure"] || null })}>
            <option value="">Not sure</option>
            <option value="full">Mostly full sun</option>
            <option value="partial">Partial sun</option>
            <option value="shade">Mostly shade</option>
          </select>
        </label>
      </div>
    </fieldset>
  );
}
