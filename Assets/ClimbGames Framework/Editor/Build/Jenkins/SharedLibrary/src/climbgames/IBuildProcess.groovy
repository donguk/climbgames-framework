package climbgames

interface IBuildProcess extends Serializable {

    void build(IBuildSettings settings)
    def deploy(IBuildSettings settings)
}