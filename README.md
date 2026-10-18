Simplified, small 1-sail square-rig boat simulation.

![Project Screenshot in Editor](Screenshots/Screenshot.png)

## Controls

- `W` / `S`: Open / close sail
- `A` / `D`: Rotate sail

---

## Current Implementation

- **Apparent Wind**: Vector difference between true wind and ship's current velocity.
- **Square-Rig Aerodynamics**: Simplified normal-pressure surface that generates force perpendicular to the sail.
- **Steering & Steerageway**: Lateral component of sail force rotates the ship. Turn sensitivity scales with forward speed.
- **Keel Dynamics & Drift**: Rotating the ship decouples the bow heading from current velocity. The keel dampens this lateral skid over time, pulling momentum back in line with the bow.
- **Propulsion & Drag**: Only the forward component of sail force accelerates the ship along its heading, damped by drag.

---

## Further Improvements

- **Aerodynamics**: Dynamic wind, sail blanketing, billowing, lift generation.
- **Water physics**: Non-linear drag (hull speed limit), speed loss during turns, buoyancy, waves, currents.
- **Hydrodynamic leeway angle**: Keel as an underwater hydrofoil generating small continuous slip angle across the wind.
- **Rudder (?)**: Would help with turning and overall control a little bit!

---

## Live Inspector Telemetry

`SailingModel` exposes serialized fields in the inspector:

- **`CurrentSpeed`**: Magnitude of velocity across the water.
- **`CurrentThrust`**: Forward thrust produced by the sail.
- **`CurrentTurnRate`**: Angular velocity in deg/s.
- **`CurrentApparentWind`**: Apparent wind magnitude felt by the sail.
- **`CurrentLateralForce`**: Lateral force driving heeling and turning torque.

