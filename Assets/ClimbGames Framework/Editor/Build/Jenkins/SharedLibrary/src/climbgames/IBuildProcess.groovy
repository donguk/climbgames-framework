package climbgames

interface IBuildProcess extends Serializable {

    void build(IBuildSettings settings)
    void deploy(IBuildSettings settings)
}