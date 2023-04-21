import { BehaviorSubject } from "rxjs";

export interface ToolbarButton {
  icon: string;
  label: string;
  separator: boolean;
  if: boolean;
  enabled: BehaviorSubject<boolean>;
  function: Function;
}
